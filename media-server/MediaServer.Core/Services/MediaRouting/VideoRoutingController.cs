using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    interface IVideoRouter
    {
        Guid CreateVideoSource();
    }

    sealed class VideoRoutingController
    {
        readonly IRoom _room;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IVideoRouter _videoRouter;

        readonly static IReadOnlyList<TrackQuality> _quality = new TrackQuality[] { TrackQuality.High, TrackQuality.Mid, TrackQuality.Low };

        public VideoRoutingController(IRoom room, IVideoRouter videoRouter)
        {
            _room = room
                ?? throw new ArgumentNullException(nameof(room));
            _videoRouter = videoRouter 
                ?? throw new ArgumentNullException(nameof(videoRouter));
        }

        /// <summary>
        /// Notify this router that a device joined the provided room.
        /// This is before any PeerConnection with the device is created.
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Should be called from the device messaging thread</remarks>
        /// <returns></returns>
        public Task DeviceJoinedRoom(IRemoteDevice device)
        {
            return _room.SignallingThread.ExecuteAsync(delegate
            {
                // For every device that join the room, we create 3 sources for high, mid, low video
                var deviceData = device.GetCustomData();
                Debug.Assert(deviceData.VideoSinks.Count == 0);
                foreach(var quality in _quality)
                {
                    deviceData.VideoSinks[quality] = new VideoSinkInfo
                    {
                        VideoSourceId = _videoRouter.CreateVideoSource()
                    };
                }
            });
        }

        /// <summary>
        /// Notify this router that a device left the provided room. 
        /// This is before any data is removed/destroyed, and before any PeerConnection is closed.
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Should be called from the device messaging thread</remarks>
        /// <returns></returns>
        public Task DeviceLeftRoom(IRemoteDevice device)
        {
            return _room.SignallingThread.ExecuteAsync(delegate
            {
                // TODO
            });
        }

        /// <summary>
        /// Notify this router that a remote track has been added 
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Must be called from signalling thread</remarks>
        /// <returns></returns>
        public void TrackAdded(IPeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            // Do not away this, as signalling thread not permitted to wait on room's thread.
            // Connect this track with one of the source
            if(string.IsNullOrWhiteSpace(rtpReceiver.Track.Id))
                throw new ArgumentNullException($"Track id is null for RTP Receiver {rtpReceiver}, Track {rtpReceiver.Track}");
            if(rtpReceiver.Track.IsAudioTrack)
                throw new ArgumentException($"Track {rtpReceiver.Track} is not a VideoTrack");


            // What
           
        }
    }
}
