#pragma once

#include "pch.h"

extern "C"
{
    EXPORT void CONVENTION
    VideoTrackAddSink(Shim::VideoTrack *video_track,
                      rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink_interface);

    EXPORT void CONVENTION
    VideoTrackRemoveSink(Shim::VideoTrack *video_track,
                         rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink_interface);
}