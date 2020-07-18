#include "pch.h"

#include "thread_sync.h"
#include "video_sink_adapter.h"

MediaSources::VideoSinkAdapter::VideoSinkAdapter(MediaSources::PassiveVideoTrackSource *video_track_source)
    : _video_track_source(video_track_source)
{
}

void MediaSources::VideoSinkAdapter::OnFrame(const webrtc::VideoFrame &frame)
{
    _video_track_source->PushVideoFrame(frame);
}
