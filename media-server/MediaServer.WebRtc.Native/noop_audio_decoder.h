#pragma once

#include "pch.h"

using namespace webrtc;

namespace NoopAudio
{
class NoopAudioDecoder final : public AudioDecoder
{
  public:
    static const size_t num_channels = 2;
    static const int sample_rate_hz = 48000;

    // Inherited via AudioDecoder
    virtual void Reset() override;
    virtual int SampleRateHz() const override;
    virtual size_t Channels() const override;
    virtual int DecodeInternal(const uint8_t *encoded,
                               size_t encoded_len,
                               int sample_rate_hz,
                               int16_t *decoded,
                               SpeechType *speech_type) override;
};
} // namespace NoopAudio