#pragma once

#include "pch.h"

using namespace webrtc;

namespace NoopAudio
{
class NoopAudioEncoderFactory final : public rtc::RefCountedObject<webrtc::AudioEncoderFactory>
{
    // Inherited via RefCountedObject
    virtual std::vector<AudioCodecSpec> GetSupportedEncoders() override;
    virtual absl::optional<AudioCodecInfo> QueryAudioEncoder(const SdpAudioFormat &format) override;
    virtual std::unique_ptr<AudioEncoder> MakeAudioEncoder(
        int payload_type,
        const SdpAudioFormat &format,
        absl::optional<AudioCodecPairId> codec_pair_id) override;
};
} // namespace NoopAudio