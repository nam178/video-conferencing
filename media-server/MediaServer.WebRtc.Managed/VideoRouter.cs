using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed
{
    sealed class VideoSource
    {
        public string ExpectedTrackId { get; set; }

        public RtpReceiver ConnectedTrack { get; set; }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }
    }

    sealed class VideoClient
    {
        public Dictionary<TrackQuality, VideoSource> VideoSources { get; } = new Dictionary<TrackQuality, VideoSource>();
    }

    sealed class VideoClientCollection
    {
        readonly Dictionary<Guid, VideoClient> _indexByVideoClientId;

        public VideoClientCollection()
        {
            _indexByVideoClientId = new Dictionary<Guid, VideoClient>();
        }

        public void AddVideoClient(Guid videoClientId)
        {
            if(_indexByVideoClientId.ContainsKey(videoClientId))
            {
                throw new InvalidOperationException($"VideoClient with id {videoClientId} already added.");
            }
            _indexByVideoClientId[videoClientId] = new VideoClient();
        }

        public VideoSource CreateVideoSource(Guid videoClientId, TrackQuality trackQuality)
        {
            RequireEntry(videoClientId);
            if(_indexByVideoClientId[videoClientId].VideoSources.ContainsKey(trackQuality))
            {
                throw new InvalidOperationException();
            }
            var t = new VideoSource();
            _indexByVideoClientId[videoClientId].VideoSources[trackQuality] = t;
            return t;
        }

        public VideoSource Get(Guid videoClientId, TrackQuality trackQuality)
        {
            RequireEntry(videoClientId);
            return _indexByVideoClientId[videoClientId].VideoSources.ContainsKey(trackQuality)
                ? _indexByVideoClientId[videoClientId].VideoSources[trackQuality]
                : null;
        }

        void RequireEntry(Guid videoClientId)
        {
            if(!_indexByVideoClientId.ContainsKey(videoClientId))
                throw new InvalidOperationException($"VideoClient {videoClientId} has not been added.");
        }

        public TrackQuality? FindQuality(Guid devieId, string trackId)
        {
            Require.NotNullOrWhiteSpace(trackId);
            RequireEntry(devieId);

            foreach(var kv in _indexByVideoClientId[devieId].VideoSources)
            {
                if(string.Equals(kv.Value.ExpectedTrackId, trackId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return kv.Key;
                }
            }
            return null;
        }

        public VideoSource FindTrack(VideoTrack track)
        {
            return _indexByVideoClientId
                .SelectMany(kv => kv.Value.VideoSources)
                .Where(kv => kv.Value.ConnectedTrack.Track == track)
                .Select(kv => kv.Value)
                .FirstOrDefault();
        }
    }

    sealed class VideoRouter
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IDispatchQueue _signallingThread;
        readonly PeerConnectionFactory _peerConnectionFactory;
        readonly VideoClientCollection _videoClients = new VideoClientCollection();
        readonly static IReadOnlyList<TrackQuality> _quality = new TrackQuality[] { TrackQuality.High, TrackQuality.Mid, TrackQuality.Low };

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
                var videoClient = _videoClients.Get(videoClientId, trackQuality);
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
                // TODO

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
                // TODO

                peerConnectionObserver.RemoteTrackAdded -= PeerConnectionObserver_RemoteTrackAdded;
                peerConnectionObserver.RemoteTrackRemoved -= PeerConnectionObserver_RemoteTrackRemoved;
            });
        }


        void PeerConnectionObserver_RemoteTrackRemoved(object sender, EventArgs<RtpReceiver> e)
        {
            throw new NotImplementedException();
        }

        void PeerConnectionObserver_RemoteTrackAdded(object sender, EventArgs<RtpReceiver> e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Notify this router that a remote track has been added
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Must be called from signalling thread, right after the track is added.</remarks>
        /// <returns></returns>
        public void AddTrack(Guid videoClientId, PeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            ValidateTrack(rtpReceiver);

            // What's the quality of this track?
            var quality = _videoClients.FindQuality(videoClientId, rtpReceiver.Track.Id);
            if(null == quality)
                throw new InvalidProgramException($"Track quality for track {rtpReceiver.Track.Id} has not been set");

            // Connect this track to the quality source
            var source = _videoClients.Get(videoClientId, quality.Value);
            ThrowForUnpreparedSource(rtpReceiver, source);

            // If there is connected track, kindly remove it first
            if(source.ConnectedTrack != null)
                ((VideoTrack)rtpReceiver.Track).RemoveSink(source.VideoSinkAdapter);
            // Then connect the new one
            ((VideoTrack)rtpReceiver.Track).AddSink(source.VideoSinkAdapter);
        }

        /// <summary>
        /// Notify this router that a remote track has been removed 
        /// </summary>
        /// <param name="videoClientId"></param>
        /// <remarks>Must be called from signalling thread, right before the track is removed</remarks>
        /// <returns></returns>
        public void RemoveTrack(Guid videoClientId, PeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            ValidateTrack(rtpReceiver);

            var source = _videoClients.FindTrack((VideoTrack)rtpReceiver.Track);
            ThrowForUnpreparedSource(rtpReceiver, source);

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

        static void ThrowForUnpreparedSource(RtpReceiver rtpReceiver, VideoSource source)
        {
            if(null == source || null == source.VideoSinkAdapter || null == source.VideoTrackSource)
            {
                throw new InvalidProgramException(
                    $"Source for track TrackId={rtpReceiver.Track.Id} has not been prepared"
                    );
            }
        }

        static void ValidateTrack(RtpReceiver rtpReceiver)
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
