#pragma once

#include "pch.h"

#include "passive_audio_track_source.h"

using namespace webrtc;

namespace MediaSources
{
class AudioSinkAdapter final : public AudioTrackSinkInterface
{
  public:
    AudioSinkAdapter(PassiveAudioTrackSource *audio_track_source);

    void OnData(const void *audio_data,
                int bits_per_sample,
                int sample_rate,
                size_t number_of_channels,
                size_t number_of_frames) override;

  private:
    PassiveAudioTrackSource *const _audio_track_source;
};
} // namespace MediaSources