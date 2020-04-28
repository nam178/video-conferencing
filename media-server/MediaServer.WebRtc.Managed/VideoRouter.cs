using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.WebRtc.Managed
{
    class VideoSource
    {
        public string ExpectedTrackId { get; set; }

        public RtpReceiver ConnectedTrack { get; set; }

        public PassiveVideoTrackSource VideoTrackSource { get; set; }

        public VideoSinkAdapter VideoSinkAdapter { get; set; }
    }

    class VideoSourceCollection
    {
        readonly Dictionary<Guid, Dictionary<TrackQuality, VideoSource>> _sources;

        public VideoSourceCollection()
        {
            _sources = new Dictionary<Guid, Dictionary<TrackQuality, VideoSource>>();
        }

        public void AddDevice(Guid deviceId)
        {
            if(_sources.ContainsKey(deviceId))
            {
                throw new InvalidOperationException($"Device with id {deviceId} already added.");
            }
            _sources[deviceId] = new Dictionary<TrackQuality, VideoSource>();
        }

        public VideoSource Create(Guid deviceId, TrackQuality trackQuality)
        {
            RequireEntry(deviceId);
            if(_sources[deviceId].ContainsKey(trackQuality))
            {
                throw new InvalidOperationException();
            }
            var t = new VideoSource();
            _sources[deviceId][trackQuality] = t;
            return t;
        }

        public VideoSource Get(Guid deviceId, TrackQuality trackQuality)
        {
            RequireEntry(deviceId);
            return _sources[deviceId].ContainsKey(trackQuality)
                ? _sources[deviceId][trackQuality]
                : null;
        }

        void RequireEntry(Guid deviceId)
        {
            if(!_sources.ContainsKey(deviceId))
                throw new InvalidOperationException($"Device {deviceId} has not been added.");
        }

        public TrackQuality? FindQuality(Guid devieId, string trackId)
        {
            Require.NotNullOrWhiteSpace(trackId);
            RequireEntry(devieId);

            foreach(var kv in _sources[devieId])
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
            return _sources
                .SelectMany(kv => kv.Value)
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
        readonly VideoSourceCollection _videoSourceCollection = new VideoSourceCollection();
        readonly static IReadOnlyList<TrackQuality> _quality = new TrackQuality[] { TrackQuality.High, TrackQuality.Mid, TrackQuality.Low };

        public VideoRouter(IDispatchQueue signallingThread, PeerConnectionFactory peerConnectionFactory)
        {
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
            _peerConnectionFactory = peerConnectionFactory 
                ?? throw new ArgumentNullException(nameof(peerConnectionFactory));
        }

        /// <summary>
        /// Notify this router that a device has joined the routing.
        /// This is before any PeerConnection with the device is created.
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Can be called from any thread, will dispatch to the signalling thread</remarks>
        /// <returns></returns>
        public Task AddDevice(Guid deviceId)
        {
            Require.NotEmpty(deviceId);

            return _signallingThread.ExecuteAsync(delegate
            {
                _videoSourceCollection.AddDevice(deviceId);
            });
        }

        /// <summary>
        /// Notify this router that a device left the current routing.
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Should be called from the device messaging thread</remarks>
        /// <returns></returns>
        public Task RemoveDevice()
        {
            return _signallingThread.ExecuteAsync(delegate
            {
                // TODO
            });
        }

        /// <summary>
        /// Notify this router that a track will be added into it with specified quality.
        /// </summary>
        /// <param name="deviceId">The device in which the track will be added</param>
        /// <param name="trackQuality"></param>
        /// <param name="trackId"></param>
        /// <remarks>Can be called from any thread, will be switched to the signalling thread.</remarks>
        /// <returns></returns>
        public Task PepareTrack(Guid deviceId, TrackQuality trackQuality, string trackId)
        {
            Require.NotEmpty(deviceId);
            Require.NotNullOrWhiteSpace(trackId);

            return _signallingThread.ExecuteAsync(delegate
            {
                // Prepare the source for this quality, if it was not created
                var trackSource = _videoSourceCollection.Get(deviceId, trackQuality);
                if(null == trackSource)
                {
                    trackSource = _videoSourceCollection.Create(deviceId, trackQuality);
                }
                trackSource.VideoTrackSource = trackSource.VideoTrackSource 
                    ?? new PassiveVideoTrackSource();
                trackSource.VideoSinkAdapter = trackSource.VideoSinkAdapter 
                    ?? new VideoSinkAdapter(_peerConnectionFactory, trackSource.VideoTrackSource, false);

                // Flag this source to say it should be 
                // linked with the provided track
                trackSource.ExpectedTrackId = trackId;
                _logger.Info($"Prepared source for track {trackId}, Quality={trackQuality}");
            });
        }

        /// <summary>
        /// Notify this router that a remote track has been added
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Must be called from signalling thread, right after the track is added.</remarks>
        /// <returns></returns>
        public void AddTrack(Guid devieId, PeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            ValidateTrack(rtpReceiver);

            // What's the quality of this track?
            var quality = _videoSourceCollection.FindQuality(devieId, rtpReceiver.Track.Id);
            if(null == quality)
            {
                _logger.Error($"Track quality for track {rtpReceiver.Track.Id} has not been set");
                return;
            }

            // Connect this track to the quality source
            var source = _videoSourceCollection.Get(devieId, quality.Value);
            if(null == source || null == source.VideoSinkAdapter || null == source.VideoTrackSource)
            {
                _logger.Error($"Source for track TrackId={rtpReceiver.Track.Id} has not been prepared");
                return;
            }
            // If there is connected track, kindly remove it first
            if(source.ConnectedTrack != null)
                ((VideoTrack)rtpReceiver.Track).RemoveSink(source.VideoSinkAdapter);
            // Then connect the new one
            ((VideoTrack)rtpReceiver.Track).AddSink(source.VideoSinkAdapter);
        }

        /// <summary>
        /// Notify this router that a remote track has been removed 
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Must be called from signalling thread, right before the track is removed</remarks>
        /// <returns></returns>
        public void TrackRemoved(Guid devieId, PeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            ValidateTrack(rtpReceiver);

            var source = _videoSourceCollection.FindTrack((VideoTrack)rtpReceiver.Track);

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
