﻿using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.WebRtc.MediaRouting
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

        public static void WhenInvalidVideoTrack(RtpReceiver rtpReceiver)
        {
            // Do not away this, as signalling thread not permitted to wait on room's thread.
            // Connect this track with one of the source
            if(string.IsNullOrWhiteSpace(rtpReceiver.Track.Id))
                throw new ArgumentNullException($"Track id is null for RTP Receiver {rtpReceiver}, Track {rtpReceiver.Track}");
            if(rtpReceiver.Track.TrackKind == MediaStreamTrack.Kind.Audio)
                throw new ArgumentException($"Track {rtpReceiver.Track} is not a VideoTrack");
        }
    }
}
