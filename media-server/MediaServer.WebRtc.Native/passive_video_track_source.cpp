#include "pch.h"

#include "passive_video_track_source.h"

using namespace webrtc;
using namespace rtc;
using namespace Video;

Video::PassiveVideoTrackSource::PassiveVideoTrackSource() : _sinks()
{
    
}

void Video::PassiveVideoTrackSource::RegisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

void Video::PassiveVideoTrackSource::UnregisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

webrtc::MediaSourceInterface::SourceState Video::PassiveVideoTrackSource::state() const
{
    return webrtc::MediaSourceInterface::SourceState::kLive;
}

bool Video::PassiveVideoTrackSource::remote() const
{
    // not remove, this is a local source
    return false;
}

void Video::PassiveVideoTrackSource::AddOrUpdateSink(VideoSinkInterface<VideoFrame> *sink,
                                                            const VideoSinkWants &wants)
{
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        if (_sinks[i] == sink)
            return;
    }
    _sinks.push_back(sink);
}

void Video::PassiveVideoTrackSource::RemoveSink(VideoSinkInterface<VideoFrame> *sink)
{
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        if (_sinks[i] == sink)
        {
            _sinks.erase(_sinks.begin() + i);
            break;
        }
    }
}

bool Video::PassiveVideoTrackSource::is_screencast() const
{
    return false;
}

absl::optional<bool> Video::PassiveVideoTrackSource::needs_denoising() const
{
    return absl::optional<bool>(false);
}

bool Video::PassiveVideoTrackSource::GetStats(Stats *stats)
{
    return false;
}

void Video::PassiveVideoTrackSource::PushVideoFrame(const VideoFrame &frame)
{
    // Must be called via the worker thread,
    // there fore it's safe to interate the sinks
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        _sinks[i]->OnFrame(frame);
    }
}
