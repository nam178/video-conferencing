using MediaServer.Common.Media;
using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Models.MediaRouting
{
    public sealed class VideoRouter : IVideoRouter
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IThread _signallingThread;
        readonly PeerConnectionFactory _peerConnectionFactory;
        readonly VideoClientCollection _videoClients = new VideoClientCollection();
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
        }

        public void AddVideoClient(Guid videoClientId)
        {
            Require.NotEmpty(videoClientId);
            _signallingThread.EnsureCurrentThread();

            var videoClient = _videoClients.AddVideoClient(videoClientId);
            _logger.Info($"Added {videoClient} into {_videoClients}");
        }

        public void SetRemoteTransceiverMetadata(TransceiverMetadata transceiverMetadata)
        {
            // Ignore audio tracks for now
            if(transceiverMetadata.Kind != MediaKind.Video)
                return;
            _signallingThread.EnsureCurrentThread();

            // Prepare the source for this quality, if it was not created
            var videoClient = _videoClients.GetOrThrow(transceiverMetadata.SourceDeviceId);
            var videoSource = videoClient.VideoSources.ContainsKey(transceiverMetadata.TrackQuality)
                ? videoClient.VideoSources[transceiverMetadata.TrackQuality]
                : null;
            if(null == videoSource)
            {
                videoSource = _videoClients.CreateVideoSource(transceiverMetadata.SourceDeviceId, transceiverMetadata.TrackQuality);
                videoSource.VideoTrackSource = new PassiveVideoTrackSource();
                videoSource.VideoSinkAdapter = new VideoSinkAdapter(videoSource.VideoTrackSource, false);
                _logger.Info($"Created {videoSource}");
                OnVideoSourceAdded(videoClient, videoSource, transceiverMetadata.TrackQuality);
            }

            // Flag this source to say it should be 
            // linked with the provided track
            videoSource.ExpectedTransceiverMid = transceiverMetadata.TransceiverMid;
            _logger.Info($"Associated {videoSource} with transceiver mid {transceiverMetadata.TransceiverMid}");
        }

        public void AddPeerConnection(Guid videoClientId, PeerConnection peerConnection)
        {
            Require.NotNull(peerConnection);
            _signallingThread.EnsureCurrentThread();

            // Add PeerConnection into VideoClient
            var videoClient = _videoClients.GetOrThrow(videoClientId);
            if(videoClient.PeerConnections.Contains(peerConnection))
            {
                throw new InvalidProgramException($"{peerConnection} already added into {videoClient}");
            }
            videoClient.PeerConnections.Add(peerConnection);
            _logger.Info($"Added {peerConnection} into {videoClient}, total PeerConnections for this client={videoClient.PeerConnections.Count}");

            // If this PeerConnetion has some existing transceivers,
            // add them.
            // TODO: listen for future transceivers and add them too, too lazy to implement this for now.
            var currenTransceivers = peerConnection.GetTransceivers();
            foreach(var transceiver in currenTransceivers)
            {
                OnRemoteTrackAdded(videoClient, peerConnection, transceiver);
            }

            // Link this PeerConnection with any existing local VideoSources
            foreach(var other in _videoClients
                .OtherThan(videoClient)
                .Where(other => other.VideoSources.ContainsKey(videoClient.DesiredMediaQuality)))
            {
                var localVideoLink = new LocalVideoLink(
                    _peerConnectionFactory,
                    other.VideoSources[videoClient.DesiredMediaQuality],
                    peerConnection);
                _localVideoLinks.Add(localVideoLink);
                _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
            }
        }

        public void RemovePeerConnection(
            Guid videoClientId,
            PeerConnection peerConnection,
            PeerConnectionObserver peerConnectionObserver)
        {
            _signallingThread.EnsureCurrentThread();
            // Remove all video links that was created for this PeerConnection
            _localVideoLinks.RemoveByPeerConnection(peerConnection);
            _logger.Info($"Removed all video links for {peerConnection} from {_localVideoLinks}");

            // Remove all video links that was created for this PeerConnetion's remote tracks
            _remoteVideoLinks.RemoveByPeerConnection(peerConnection);
            _logger.Info($"Removed all video links for {peerConnection} from {_remoteVideoLinks}");

            // Remove this PeerConnection from VideoClient
            var videoClient = _videoClients.GetOrThrow(videoClientId);
            var removed = videoClient.PeerConnections.Remove(peerConnection);
            if(!removed)
                throw new InvalidProgramException("PeerConnection did not removed from memory");
            _logger.Info($"Removed {peerConnection} from {videoClient}, remaining PeerConnections for this client={videoClient.PeerConnections.Count}");
        }

        public void RemoveVideoClient(Guid videoClientId)
        {
            _signallingThread.EnsureCurrentThread();
            // Remove the video sources
            var videoClient = _videoClients.GetOrThrow(videoClientId);
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
        /// Notify this router that a remote track has been removed.
        /// This is not currently being used.
        /// We assume clients declare 2 transmiters for audio and video upfront, and will never change it.
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
                .Where(other => other.DesiredMediaQuality == mediaQuality && other.PeerConnections.Count > 0))
            {
                var localVideoLink = new LocalVideoLink(
                    _peerConnectionFactory,
                    videoSource,
                    otherVideoClient.PeerConnections[0]);
                _localVideoLinks.Add(localVideoLink);
                _logger.Debug($"Added {localVideoLink} into {_localVideoLinks}");
            }
        }

        public void ClearFrozenTransceivers(Guid videoClientId, Guid peerConnectionId)
        {
            _signallingThread.EnsureCurrentThread();

            var videoClient = _videoClients.GetOrThrow(videoClientId);
            var peerConnection = videoClient.GetPeerConnectionOrThrow(peerConnectionId);
            var transceivers = peerConnection.GetTransceivers();
            var count = 0;
            for(int i = 0; i < transceivers.Count; i++)
            {
                if(transceivers[i].ReusabilityState == TransceiverReusabilityState.Fronzen)
                {
                    transceivers[i].ToAvailableState();
                    count++;
                }
            }
            if(count > 0)
            {
                _logger.Info(
                    $"Made {count} frozen transceiver(s) available for {peerConnection}, " +
                    $"total transceivers: {transceivers.Count}");
            }
        }

        public IReadOnlyList<TransceiverMetadata> GetLocalTransceiverMetadata(Guid videoClientId, Guid peerConnectionId)
        {
            _signallingThread.EnsureCurrentThread();
            var videoClient = _videoClients.GetOrThrow(videoClientId);
            var peerConnection = videoClient.GetPeerConnectionOrThrow(peerConnectionId);
            var transceivers = peerConnection.GetTransceivers();
            if(null == transceivers)
                throw new NullReferenceException(nameof(transceivers));

            // We only generate transceiver metadata those are:
            //  1. Currently being used (TransceiverReusabilityState busy)
            //  2. Has been negotiated (Mid is not null)
            var result = new List<TransceiverMetadata>();
            for(var i = 0; i < transceivers.Count; i++)
            {
                if(transceivers[i].ReusabilityState == TransceiverReusabilityState.Busy
                    && false == string.IsNullOrWhiteSpace(transceivers[i].Mid))
                {
                    if(null == transceivers[i].CustomData)
                        throw new InvalidProgramException("Transceiver custom data is  NULL");
                    var localVideoLink = transceivers[i].CustomData as LocalVideoLink;
                    if(null == transceivers[i].CustomData)
                        throw new InvalidProgramException($"Transceiver custom data is not {nameof(LocalVideoLink)}");

                    result.Add(new TransceiverMetadata(
                        transceivers[i].Mid,
                        videoClient.DesiredMediaQuality,
                        transceivers[i].MediaKind,
                        localVideoLink.VideoSource.VideoClient.Id));
                }
            }
            return result;
        }
    }
}
