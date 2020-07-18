#include "pch.h"

#include "audio_sink_adapter.h"

MediaSources::AudioSinkAdapter::AudioSinkAdapter(PassiveAudioTrackSource *audio_track_source)
    : _audio_track_source(audio_track_source)
{
}

inline void MediaSources::AudioSinkAdapter::OnData(const void *audio_data,
                                                   int bits_per_sample,
                                                   int sample_rate,
                                                   size_t number_of_channels,
                                                   size_t number_of_frames)
{
    _audio_track_source->PushData(
        audio_data, bits_per_sample, sample_rate, number_of_channels, number_of_frames);
}
