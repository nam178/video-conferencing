#include "pch.h"

#include "video_track.h"
#include "video_track_interops.h"

void CONVENTION VideoTrackAddSink(void *video_track, void *video_sink_interface)
{
    auto video_sink =
        StaticCastOrThrow<rtc::VideoSinkInterface<webrtc::VideoFrame>>(video_sink_interface);

    StaticCastOrThrow<Wrappers::VideoTrack>(video_track)->AddSink(video_sink);
}

void CONVENTION VideoTrackRemoveSink(void *video_track, void *video_sink_interface)
{
    auto video_sink =
        StaticCastOrThrow<rtc::VideoSinkInterface<webrtc::VideoFrame>>(video_sink_interface);

    StaticCastOrThrow<Wrappers::VideoTrack>(video_track)->RemoveSink(video_sink);
}

