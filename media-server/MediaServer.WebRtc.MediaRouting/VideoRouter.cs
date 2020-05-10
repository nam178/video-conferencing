using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.MediaRouting
{
    public sealed class VideoRouter : IVideoRouter
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IThread _signallingThread;
        readonly PeerConnectionFactory _peerConnectionFactory;
        readonly VideoClientCollection _videoClients = new VideoClientCollection();
        readonly VideoRouterEventHandler _eventHandler;
        readonly LocalVideoLinkCollection _localVideoLinks = new LocalVideoLinkCollection();
        readonly RemoteVideoLinkCollection _remoteVideoLinks = new RemoteVideoLinkCollection();

        public VideoRouter(
            IThread signallingThread,
            PeerConnectionFactory peerConnectionFactory)
        {
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            _peerConnectionFactory = peerConnectionFactory
                ?? throw new ArgumentNullException(nameof(peerConnectionFactory));
            _eventHandler = new VideoRouterEventHandler(_signallingThread, this, _videoClients);
        }

        public Task AddVideoClientAsync(Guid videoClientId)
        {
            Require.NotEmpty(videoClientId);

            return _signallingThread.ExecuteAsync(delegate
            {
                var videoClient = _videoClients.AddVideoClient(videoClientId);
                _logger.Info($"Added {videoClient} into {_videoClients}");
            });
        }

        public Task PrepareAsync(Guid videoClientId, MediaQuality mediaQuality, MediaKind mediaKind, string transceiverMid)
        {
            Require.NotEmpty(videoClientId);
            Require.NotNullOrWhiteSpace(transceiverMid);

            // Ignore audio tracks for now
            if(mediaKind != MediaKind.Video)
                return Task.CompletedTask;

            return _signallingThread.ExecuteAsync(delegate
            {
                // Prepare the source for this quality, if it was not created
                var videoClient = _videoClients.Get(videoClientId);
                var videoSource = videoClient.VideoSources.ContainsKey(mediaQuality)
                    ? videoClient.VideoSources[mediaQuality]
                    : null;
                if(null == videoSource)
                {
                    videoSource = _videoClients.CreateVideoSource(videoClientId, mediaQuality);
                    videoSource.VideoTrackSource = new PassiveVideoTrackSource();
                    videoSource.VideoSinkAdapter = new VideoSinkAdapter(videoSource.VideoTrackSource, false);
                    _logger.Info($"Created {videoSource}");
                    OnVideoSourceAdded(videoClient, videoSource, mediaQuality);
                }

                // Flag this source to say it should be 
                // linked with the provided track
                videoSource.ExpectedTransceiverMid = transceiverMid;
                _logger.Info($"Associated {videoSource} with transceiver mid {transceiverMid}");
            });
        }

        public Task AddPeerConnectionAsync(Guid videoClientId, PeerConnection peerConnection)
        {
            Require.NotNull(peerConnection);

            return _signallingThread.ExecuteAsync(delegate
            {
                // Add PeerConnection into VideoClient
                var videoClient = _videoClients.Get(videoClientId);
                if(videoClient.PeerConnections.Contains(peerConnection))
                {
                    throw new InvalidProgramException($"{peerConnection} already added into {videoClient}");
                }
                videoClient.PeerConnections.Add(peerConnection);
                _logger.Info($"Added {peerConnection} into {videoClient}, total PeerConnections for this client={videoClient.PeerConnections.Count}");

                // This PeerConnection must be added 
                // right before it is created, therefore it must has no remote tracks
                foreach(var remoteTrack in peerConnection.Observer.RemoteTracks)
                {
                    OnRemoteTrackAdded(videoClient, peerConnection, remoteTrack);
                }

                // Start listening to it for track events
                peerConnection.Observer.RemoteTrackAdded += _eventHandler.RemoteTrackAdded;
                peerConnection.Observer.RemoteTrackRemoved += _eventHandler.RemoteTrackRemoved;

                // Link this PeerConnection with any existing local VideoSources
                foreach(var other in _videoClients
                    .OtherThan(videoClient)
                    .Where(other => other.VideoSources.ContainsKey(videoClient.DesiredVideoQuality)))
                {
                    var localVideoLink = new LocalVideoLink(
                        _peerConnectionFactory,
                        other.VideoSources[videoClient.DesiredVideoQuality],
                        peerConnection);
                    _localVideoLinks.Add(localVideoLink);
                    _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
                }
            });
        }

        public Task RemovePeerConnectionAsync(
            Guid videoClientId,
            PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver)
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                // Remove all video links that was created for this PeerConnection
                _localVideoLinks.RemoveByPeerConnection(peerConnection);
                _logger.Info($"Removed all video links for {peerConnection} from {_localVideoLinks}");

                // Remove all video links that was created for this PeerConnetion's remote tracks
                _remoteVideoLinks.RemoveByPeerConnection(peerConnection);
                _logger.Info($"Removed all video links for {peerConnection} from {_remoteVideoLinks}");

                // Here basically undo the things we did in AddPeerConnection(),
                // in reverse order.
                // No longer interested in its track events
                peerConnectionObserver.RemoteTrackAdded -= _eventHandler.RemoteTrackAdded;
                peerConnectionObserver.RemoteTrackRemoved -= _eventHandler.RemoteTrackRemoved;

                // Remove this PeerConnection from VideoClient
                var videoClient = _videoClients.Get(videoClientId);
                var removed = videoClient.PeerConnections.Remove(peerConnection);
                if(!removed)
                    throw new InvalidProgramException("PeerConnection did not removed from memory");
                _logger.Info($"Removed {peerConnection} from {videoClient}, remaining PeerConnections for this client={videoClient.PeerConnections.Count}");
            });
        }

        public Task RemoveVideoClientAsync(Guid videoClientId)
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                // Remove the video sources
                var videoClient = _videoClients.Get(videoClientId);
                if(videoClient.PeerConnections.Count > 0)
                    throw new InvalidProgramException("VideoClient's PeerConnections were not removed");

                // Remove each video source
                foreach(var kv in videoClient.VideoSources)
                {
                    var videoSource = kv.Value;

                    if(videoSource.VideoTrackSource == null)
                        throw new InvalidProgramException($"{nameof(videoSource.VideoTrackSource)} is null");
                    if(videoSource.VideoSinkAdapter == null)
                        throw new InvalidProgramException($"{nameof(videoSource.VideoSinkAdapter)} is null");

                    using(videoSource.VideoTrackSource)
                    using(videoSource.VideoSinkAdapter)
                    {
                        // At this point, there must be no remote link to this VideoSource,
                        // because they are removed when PeerConnection closes.
                        // If not, it's a programmer mistake.
                        if(_remoteVideoLinks.Exists(videoSource))
                        {
                            throw new InvalidProgramException(
                                "All remote VideoSource must be removed at the time VideoClient left.");
                        }

                        // Disconnect all any remote video links.
                        _localVideoLinks.RemoveByVideoSource(videoSource);
                        _logger.Info($"Removed {videoSource} from {_localVideoLinks}");
                    }
                }

                // Remove the video client
                _videoClients.Remove(videoClientId);
                _logger.Info($"Removed client {videoClient} from {_videoClients}");
            });
        }

        /// <summary>
        /// Notify this router that a remote track has been added
        /// </summary>
        /// <remarks>Must be called from signalling thread, right after the track is added.</remarks>
        /// <returns></returns>
        internal void OnRemoteTrackAdded(VideoClient videoClient, PeerConnection peerConnection, RtpTransceiver transceiver)
        {
            _signallingThread.EnsureCurrentThread();
            Require.NotNull(peerConnection);
            VideoRouterThrowHelper.WhenInvalidReceiver(transceiver);

            // Ignore audio tracks for now
            if(transceiver.Receiver.Track.Kind != MediaKind.Video)
                return;

            // What's the video source that this track should be connected to?
            var transceiverMid = transceiver.Mid;
            var videoSource = videoClient.VideoSources
                .FirstOrDefault(kv => string.Equals(kv.Value.ExpectedTransceiverMid, transceiverMid, StringComparison.InvariantCultureIgnoreCase))
                .Value;
            if(null == videoSource)
                throw new InvalidOperationException($"Could not find VideoSource for transceiver {transceiver}");

            VideoRouterThrowHelper.WhenSourceIsEmpty(videoSource, transceiver.Receiver);

            var remoteVideoLink = new RemoteVideoLink(peerConnection, videoSource, transceiver.Receiver);
            _remoteVideoLinks.AddOrUpdate(remoteVideoLink);
            _logger.Info($"Added {remoteVideoLink} into {_remoteVideoLinks}");
        }

        /// <summary>
        /// Notify this router that a remote track has been removed 
        /// </summary>
        /// <remarks>Must be called from signalling thread, right before the track is removed</remarks>
        /// <returns></returns>
        internal void OnRemoteTrackRemoved(RtpTransceiver transceiver)
        {
            _signallingThread.EnsureCurrentThread();
            VideoRouterThrowHelper.WhenInvalidReceiver(transceiver);

            _remoteVideoLinks.RemoveByRemoteTrack(transceiver.Receiver);
            _logger.Info($"Removed {transceiver.Receiver} from {_remoteVideoLinks}");
        }

        void OnVideoSourceAdded(VideoClient videoClient, VideoSource videoSource, MediaQuality mediaQuality)
        {
            _signallingThread.EnsureCurrentThread();

            // As new video source is added, 
            // we'll have to connect this source to any existing sending-PeerConnection
            foreach(var otherVideoClient in _videoClients
                .OtherThan(videoClient)
                .Where(other => other.DesiredVideoQuality == mediaQuality && other.PeerConnections.Count > 0))
            {
                var localVideoLink = new LocalVideoLink(
                    _peerConnectionFactory,
                    videoSource,
                    otherVideoClient.PeerConnections[0]);
                _localVideoLinks.Add(localVideoLink);
                _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
            }
        }
    }
}
