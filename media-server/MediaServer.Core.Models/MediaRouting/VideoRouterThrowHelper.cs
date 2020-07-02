using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Models.MediaRouting
{
    static class VideoRouterThrowHelper
    {
        public static void WhenSourceIsEmpty(VideoSource source, RtpReceiver rtpReceiver)
        {
            if(null == source || null == source.VideoSinkAdapter || null == source.VideoTrackSource)
            {
                throw new InvalidProgramException(
                    $"Source for track TrackId={rtpReceiver.Track.Id} has not been prepared"
                    );
            }
        }

        public static void WhenInvalidReceiver(RtpTransceiver transceiver)
        {
            var track = transceiver.Receiver.Track;
            if(track == null)
                throw new InvalidProgramException("Track is NULL");

            if(string.IsNullOrWhiteSpace(track.Id))
                throw new ArgumentNullException($"Track id is null for {transceiver}, Track {transceiver.Receiver.Track}");
        }
    }
}
