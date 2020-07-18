#include "pch.h"

#include "passive_audio_track_source.h"

void MediaSources::PassiveAudioTrackSource::PushData(const void *audio_data,
                                                     int bits_per_sample,
                                                     int sample_rate,
                                                     size_t number_of_channels,
                                                     size_t number_of_frames)
{
    // TODO
}

void MediaSources::PassiveAudioTrackSource::RegisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

void MediaSources::PassiveAudioTrackSource::UnregisterObserver(ObserverInterface *observer)
{
    // nothing to observe here
}

MediaSourceInterface::SourceState MediaSources::PassiveAudioTrackSource::state() const
{
    return SourceState::kLive;
}

bool MediaSources::PassiveAudioTrackSource::remote() const
{
    return false;
}

void MediaSources::PassiveAudioTrackSource::AddSink(AudioTrackSinkInterface *sink)
{
    // signalling thread
    _sinks.push_back(sink);
}

void MediaSources::PassiveAudioTrackSource::RemoveSink(AudioTrackSinkInterface *sink)
{
    // signalling thread
    auto pos = std::find(_sinks.begin(), _sinks.end(), sink);
    if(pos != _sinks.end())
    {
        _sinks.erase(pos);
    }
}
