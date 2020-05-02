#include "pch.h"

#include "thread_sync.h"
#include "video_sink_adapter.h"

Video::VideoSinkAdapter::VideoSinkAdapter(Video::PassiveVideoTrackSource *video_track_source)
    : _video_track_source(video_track_source)
{
}

void Video::VideoSinkAdapter::OnFrame(const webrtc::VideoFrame &frame)
{
    _video_track_source->PushVideoFrame(frame);
}
