#include "pch.h"

#include "noop_audio_encoder_factory.h"

using namespace webrtc;

std::vector<AudioCodecSpec> NoopAudio::NoopAudioEncoderFactory::GetSupportedEncoders()
{
    return std::vector<AudioCodecSpec>();
}

absl::optional<AudioCodecInfo> NoopAudio::NoopAudioEncoderFactory::QueryAudioEncoder(
    const SdpAudioFormat &format)
{
    return absl::optional<AudioCodecInfo>();
}

std::unique_ptr<AudioEncoder> NoopAudio::NoopAudioEncoderFactory::MakeAudioEncoder(
    int payload_type,
    const SdpAudioFormat &format,
    absl::optional<AudioCodecPairId> codec_pair_id)
{
    return std::unique_ptr<AudioEncoder>();
}
