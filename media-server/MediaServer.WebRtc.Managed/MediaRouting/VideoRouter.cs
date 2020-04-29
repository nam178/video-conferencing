using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed.MediaRouting
{
    sealed class VideoRouter
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IDispatchQueue _signallingThread;
        readonly PeerConnectionFactory _peerConnectionFactory;
        readonly VideoClientCollection _videoClients = new VideoClientCollection();

        public VideoRouter(IDispatchQueue signallingThread, PeerConnectionFactory peerConnectionFactory)
        {
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            _peerConnectionFactory = peerConnectionFactory
                ?? throw new ArgumentNullException(nameof(peerConnectionFactory));
        }

        /// <summary>
        /// Notify this router that a video client has joined the routing.
        /// This is before any PeerConnection with the video client is created.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Can be called from any thread, will dispatch to the signalling thread</remarks>
        /// <returns></returns>
        public Task AddVideoClient(Guid videoClientId)
        {
            Require.NotEmpty(videoClientId);

            return _signallingThread.ExecuteAsync(delegate
            {
                _videoClients.AddVideoClient(videoClientId);
            });
        }

        /// <summary>
        /// Notify this router that a track will be added into it with specified quality.
        /// </summary>
        /// <param name="videoClientId">The video client in which the track will be added</param>
        /// <param name="trackQuality"></param>
        /// <param name="trackId"></param>
        /// <remarks>Can be called from any thread, will be switched to the signalling thread.</remarks>
        /// <returns></returns>
        public Task PepareTrack(Guid videoClientId, TrackQuality trackQuality, string trackId)
        {
            Require.NotEmpty(videoClientId);
            Require.NotNullOrWhiteSpace(trackId);

            return _signallingThread.ExecuteAsync(delegate
            {
                // Prepare the source for this quality, if it was not created
                var videoClient = _videoClients.FindVideoSource(videoClientId, trackQuality);
                if(null == videoClient)
                {
                    videoClient = _videoClients.CreateVideoSource(videoClientId, trackQuality);
                }
                videoClient.VideoTrackSource = videoClient.VideoTrackSource
                    ?? new PassiveVideoTrackSource();
                videoClient.VideoSinkAdapter = videoClient.VideoSinkAdapter
                    ?? new VideoSinkAdapter(_peerConnectionFactory, videoClient.VideoTrackSource, false);

                // Flag this source to say it should be 
                // linked with the provided track
                videoClient.ExpectedTrackId = trackId;
                _logger.Info($"Prepared source for track {trackId}, Quality={trackQuality}");
            });
        }

        /// <summary>
        /// Notify this router that a PeerConnection is created.
        /// Must be called right after the PeerConnection is created, before SetRemoteSessionDescription()
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <param name="peerConnection"></param>
        /// <remarks>Can be called on any thread</remarks>
        public void AddPeerConnection(Guid videoClientId, PeerConnection peerConnection, PeerConnectionObserver peerConnectionObserver)
        {
            _signallingThread.ExecuteAsync(delegate
            {
                var videoClient = _videoClients.Get(videoClientId);
                videoClient.PeerConnections.Add(new PeerConnectionEntry(peerConnection, peerConnectionObserver));

                peerConnectionObserver.RemoteTrackAdded += PeerConnectionObserver_RemoteTrackAdded;
                peerConnectionObserver.RemoteTrackRemoved += PeerConnectionObserver_RemoteTrackRemoved;
            });
        }

        /// <summary>
        /// Notify this router that a PeerConnection will be removed/closed.
        /// Must be called right before the PeerConnection is closed.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <param name="peerConnection"></param>
        /// <remarks>Can be called on any thread</remarks>
        public void RemovePeerConnection(Guid videoClientId, PeerConnection peerConnection, PeerConnectionObserver peerConnectionObserver)
        {
            _signallingThread.ExecuteAsync(delegate
            {
                peerConnectionObserver.RemoteTrackAdded -= PeerConnectionObserver_RemoteTrackAdded;
                peerConnectionObserver.RemoteTrackRemoved -= PeerConnectionObserver_RemoteTrackRemoved;

                var videoClient = _videoClients.Get(videoClientId);
                var removed = videoClient.PeerConnections.RemoveAll(entry => entry.PeerConnection == peerConnection);
                if(removed == 0)
                    throw new InvalidProgramException("PeerConnection did not removed from memory");
            });
        }

        void PeerConnectionObserver_RemoteTrackAdded(object sender, EventArgs<RtpReceiver> e)
        {
            var observer = (PeerConnectionObserver)sender;
            var videoClient = _videoClients.FindByObserver(observer, out var peerConnection);
            if(null == videoClient)
                throw new InvalidProgramException($"VideoClient not found for observer with track={e.Payload}");

            AddTrack(videoClient, peerConnection, e.Payload);
        }

        void PeerConnectionObserver_RemoteTrackRemoved(object sender, EventArgs<RtpReceiver> e)
        {
            var observer = (PeerConnectionObserver)sender;
            var videoClient = _videoClients.FindByObserver(observer, out var peerConnection);
            if(null == videoClient)
                throw new InvalidProgramException($"VideoClient not found for observer with track={e.Payload}");

            RemoveTrack(videoClient, peerConnection, e.Payload);
        }


        /// <summary>
        /// Notify this router that a remote track has been added
        /// </summary>
        /// <remarks>Must be called from signalling thread, right after the track is added.</remarks>
        /// <returns></returns>
        internal void AddTrack(VideoClient videoClient, PeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            ThrowWhenInvalidVideoTrack(rtpReceiver);

            // What's the video source that this track should be connected to?
            var videoSource = videoClient.VideoSources
                .FirstOrDefault(kv => string.Equals(kv.Value.ExpectedTrackId, rtpReceiver.Track.Id, StringComparison.InvariantCultureIgnoreCase))
                .Value;
            if(null == videoSource)
                throw new InvalidProgramException($"Track quality for track {rtpReceiver.Track.Id} has not been set");

            ThrowWhenSourceIsEmpty(videoSource, rtpReceiver);

            // Connect this track to the desired source:
            // If there is connected track, kindly remove it first
            if(videoSource.ConnectedTrack != null)
                ((VideoTrack)rtpReceiver.Track).RemoveSink(videoSource.VideoSinkAdapter);

            // Then connect the new one
            ((VideoTrack)rtpReceiver.Track).AddSink(videoSource.VideoSinkAdapter);
        }

        /// <summary>
        /// Notify this router that a remote track has been removed 
        /// </summary>
        /// <remarks>Must be called from signalling thread, right before the track is removed</remarks>
        /// <returns></returns>
        internal void RemoveTrack(VideoClient videoClient, PeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            ThrowWhenInvalidVideoTrack(rtpReceiver);

            // Find the source that connected to this track
            var source = videoClient.VideoSources.FirstOrDefault(kv => kv.Value.ConnectedTrack == rtpReceiver).Value;
            ThrowWhenSourceIsEmpty(source, rtpReceiver);

            // And remove this track from it
            ((VideoTrack)rtpReceiver.Track).RemoveSink(source.VideoSinkAdapter);
            source.ConnectedTrack = null;
        }

        /// <summary>
        /// Notify this router that a video client has left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Can be called from any thread</remarks>
        /// <returns></returns>
        public Task RemoveVideoClient(Guid videoClientId)
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                // TODO
                throw new NotImplementedException();
            });
        }

        static void ThrowWhenSourceIsEmpty(VideoSource source, RtpReceiver rtpReceiver)
        {
            if(null == source || null == source.VideoSinkAdapter || null == source.VideoTrackSource)
            {
                throw new InvalidProgramException(
                    $"Source for track TrackId={rtpReceiver.Track.Id} has not been prepared"
                    );
            }
        }

        static void ThrowWhenInvalidVideoTrack(RtpReceiver rtpReceiver)
        {
            // Do not away this, as signalling thread not permitted to wait on room's thread.
            // Connect this track with one of the source
            if(string.IsNullOrWhiteSpace(rtpReceiver.Track.Id))
                throw new ArgumentNullException($"Track id is null for RTP Receiver {rtpReceiver}, Track {rtpReceiver.Track}");
            if(rtpReceiver.Track.IsAudioTrack)
                throw new ArgumentException($"Track {rtpReceiver.Track} is not a VideoTrack");
        }
    }
}
