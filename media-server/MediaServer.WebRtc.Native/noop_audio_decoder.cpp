#include "pch.h"

#include "noop_audio_decoder.h"

void NoopAudio::NoopAudioDecoder::Reset()
{
}

int NoopAudio::NoopAudioDecoder::SampleRateHz() const
{
    return sample_rate_hz;
}

size_t NoopAudio::NoopAudioDecoder::Channels() const
{
    return num_channels;
}

int NoopAudio::NoopAudioDecoder::DecodeInternal(const uint8_t *encoded,
                                                size_t encoded_len,
                                                int sample_rate_hz,
                                                int16_t *decoded,
                                                SpeechType *speech_type)
{
    *speech_type = SpeechType::kSpeech;
    for(size_t i = 0; i < encoded_len; i++)
    {
        decoded[i] = encoded[i];
    }
    return encoded_len;
}
