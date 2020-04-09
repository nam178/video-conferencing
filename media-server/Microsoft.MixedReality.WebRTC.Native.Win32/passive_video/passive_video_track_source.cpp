#include "pch.h"

#include "passive_video_track_source.h"

using namespace webrtc;
using namespace rtc;
using namespace PassiveVideo;

PassiveVideo::PassiveVideoTrackSource::PassiveVideoTrackSource() : _sinks()
{
    
}

void PassiveVideo::PassiveVideoTrackSource::RegisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

void PassiveVideo::PassiveVideoTrackSource::UnregisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

webrtc::MediaSourceInterface::SourceState PassiveVideo::PassiveVideoTrackSource::state() const
{
    return webrtc::MediaSourceInterface::SourceState::kLive;
}

bool PassiveVideo::PassiveVideoTrackSource::remote() const
{
    // not remove, this is a local source
    return false;
}

void PassiveVideo::PassiveVideoTrackSource::AddOrUpdateSink(VideoSinkInterface<VideoFrame> *sink,
                                                            const VideoSinkWants &wants)
{
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        if (_sinks[i] == sink)
            return;
    }
    _sinks.push_back(sink);
}

void PassiveVideo::PassiveVideoTrackSource::RemoveSink(VideoSinkInterface<VideoFrame> *sink)
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

bool PassiveVideo::PassiveVideoTrackSource::is_screencast() const
{
    return false;
}

absl::optional<bool> PassiveVideo::PassiveVideoTrackSource::needs_denoising() const
{
    return absl::optional<bool>(false);
}

bool PassiveVideo::PassiveVideoTrackSource::GetStats(Stats *stats)
{
    stats->input_width = 320;
    stats->input_height = 320;
    return true;
}

void PassiveVideo::PassiveVideoTrackSource::PushVideoFrame(const VideoFrame &frame)
{
    _total++;
    if((_total % 30) == 0)
    {
        RTC_LOG(LS_VERBOSE) << "Total pushed to source: " << _total;
    }

    // Must be called via the worker thread,
    // there fore it's safe to interate the sinks
    for (size_t i = 0; i < _sinks.size(); i++)
    {
        _sinks[i]->OnFrame(frame);
    }
}
