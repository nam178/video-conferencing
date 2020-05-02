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
        readonly IDispatchQueue _signallingThread;
        readonly PeerConnectionFactory _peerConnectionFactory;
        readonly VideoClientCollection _videoClients = new VideoClientCollection();
        readonly VideoRouterEventHandler _eventHandler;
        readonly LocalVideoLinkCollection _localVideoLinks = new LocalVideoLinkCollection();
        readonly RemoteVideoLinkCollection _remoteVideoLinks = new RemoteVideoLinkCollection();

        public VideoRouter(
            IDispatchQueue signallingThread,
            PeerConnectionFactory peerConnectionFactory)
        {
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            _peerConnectionFactory = peerConnectionFactory
                ?? throw new ArgumentNullException(nameof(peerConnectionFactory));
            _eventHandler = new VideoRouterEventHandler(this, _videoClients);
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

        public Task PepareTrackAsync(Guid videoClientId, TrackQuality trackQuality, string trackId)
        {
            Require.NotEmpty(videoClientId);
            Require.NotNullOrWhiteSpace(trackId);

            return _signallingThread.ExecuteAsync(delegate
            {
                // Prepare the source for this quality, if it was not created
                var videoClient = _videoClients.Get(videoClientId);
                var videoSource = videoClient.VideoSources.ContainsKey(trackQuality)
                    ? videoClient.VideoSources[trackQuality]
                    : null;
                if(null == videoSource)
                {
                    videoSource = _videoClients.CreateVideoSource(videoClientId, trackQuality);
                    videoSource.VideoTrackSource = new PassiveVideoTrackSource();
                    videoSource.VideoSinkAdapter = new VideoSinkAdapter(
                        _peerConnectionFactory,
                        videoSource.VideoTrackSource, false);
                    _logger.Info($"Created {videoSource}");

                    // As new video source is added, 
                    // we'll have to connect this source to any existing primary PeerConnection
                    foreach(var otherVideoClient in _videoClients
                        .OtherThan(videoClient)
                        .Where(other => other.DesiredVideoQuality == trackQuality
                            && other.PeerConnections.Count > 0))
                    {
                        var localVideoLink = new LocalVideoLink(
                            _peerConnectionFactory, videoSource,
                            otherVideoClient.PeerConnections[0].PeerConnection);
                        _localVideoLinks.Add(localVideoLink);
                        _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
                    }
                }


                // Flag this source to say it should be 
                // linked with the provided track
                videoSource.ExpectedTrackId = trackId;
                _logger.Info($"Associated {videoSource} with track {trackId}");
            });
        }

        public Task AddPeerConnectionAsync(
            Guid videoClientId,
            PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver)
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                // Add PeerConnection into VideoClient
                var videoClient = _videoClients.Get(videoClientId);
                var peerConnectionEntry = new PeerConnectionEntry(peerConnection, peerConnectionObserver);
                videoClient.PeerConnections.Add(peerConnectionEntry);
                _logger.Info($"Added {peerConnection} into {videoClient}, total PeerConnections for this client={videoClient.PeerConnections.Count}");

                // Start listening to it for track events
                peerConnectionObserver.RemoteTrackAdded += _eventHandler.RemoteTrackAdded;
                peerConnectionObserver.RemoteTrackRemoved += _eventHandler.RemoteTrackRemoved;

                // If this is the primary PeerConnection
                // For each of other people's video source, add one remote track for this PeerConnection.
                if(videoClient.IsPrimaryPeerConnection(peerConnection))
                {
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
                var removed = videoClient.PeerConnections.RemoveAll(entry => entry.PeerConnection == peerConnection);
                if(removed == 0)
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
        internal void AddRemoteTrack(PeerConnection peerConnection, VideoClient videoClient, RtpReceiver rtpReceiver)
        {
            if(peerConnection is null)
                throw new ArgumentNullException(nameof(peerConnection));
            VideoRouterThrowHelper.WhenInvalidVideoTrack(rtpReceiver);

            // What's the video source that this track should be connected to?
            var videoSource = videoClient.VideoSources
                .FirstOrDefault(kv => string.Equals(kv.Value.ExpectedTrackId, rtpReceiver.Track.Id, StringComparison.InvariantCultureIgnoreCase))
                .Value;
            if(null == videoSource)
                throw new InvalidOperationException($"Track quality for track {rtpReceiver.Track.Id} has not been set");

            VideoRouterThrowHelper.WhenSourceIsEmpty(videoSource, rtpReceiver);

            var remoteVideoLink = new RemoteVideoLink(peerConnection, videoSource, rtpReceiver);
            _remoteVideoLinks.AddOrUpdate(remoteVideoLink);
            _logger.Info($"Added {remoteVideoLink} into {_remoteVideoLinks}");
        }

        /// <summary>
        /// Notify this router that a remote track has been removed 
        /// </summary>
        /// <remarks>Must be called from signalling thread, right before the track is removed</remarks>
        /// <returns></returns>
        internal void RemoveRemoteTrack(RtpReceiver rtpReceiver)
        {
            VideoRouterThrowHelper.WhenInvalidVideoTrack(rtpReceiver);

            _remoteVideoLinks.RemoveByRemoteTrack(rtpReceiver);
            _logger.Info($"Removed {rtpReceiver} from {_remoteVideoLinks}");
        }
    }
}
