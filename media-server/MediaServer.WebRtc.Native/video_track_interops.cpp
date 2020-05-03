#include "pch.h"

#include "video_track.h"
#include "video_track_interops.h"

void CONVENTION VideoTrackAddSink(Wrappers::VideoTrack *video_track,
                                  rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink_interface)
{
    video_track->AddSink(video_sink_interface);
}

void CONVENTION
VideoTrackRemoveSink(Wrappers::VideoTrack *video_track,
                     rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink_interface)
{
    video_track->RemoveSink(video_sink_interface);
}
