#include "pch.h"

#include "passive_video_track_source.h"

using namespace webrtc;
using namespace rtc;
using namespace MediaSources;

MediaSources::PassiveVideoTrackSource::PassiveVideoTrackSource() : _sinks()
{
    
}

void MediaSources::PassiveVideoTrackSource::RegisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

void MediaSources::PassiveVideoTrackSource::UnregisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

webrtc::MediaSourceInterface::SourceState MediaSources::PassiveVideoTrackSource::state() const
{
    return webrtc::MediaSourceInterface::SourceState::kLive;
}

bool MediaSources::PassiveVideoTrackSource::remote() const
{
    // not remove, this is a local source
    return false;
}

void MediaSources::PassiveVideoTrackSource::AddOrUpdateSink(VideoSinkInterface<VideoFrame> *sink,
                                                            const VideoSinkWants &wants)
{
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        if (_sinks[i] == sink)
            return;
    }
    _sinks.push_back(sink);
}

void MediaSources::PassiveVideoTrackSource::RemoveSink(VideoSinkInterface<VideoFrame> *sink)
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

bool MediaSources::PassiveVideoTrackSource::is_screencast() const
{
    return false;
}

absl::optional<bool> MediaSources::PassiveVideoTrackSource::needs_denoising() const
{
    return absl::optional<bool>(false);
}

bool MediaSources::PassiveVideoTrackSource::GetStats(Stats *stats)
{
    return false;
}

void MediaSources::PassiveVideoTrackSource::PushVideoFrame(const VideoFrame &frame)
{
    // Must be called via the worker thread,
    // there fore it's safe to interate the sinks
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        _sinks[i]->OnFrame(frame);
    }
}
