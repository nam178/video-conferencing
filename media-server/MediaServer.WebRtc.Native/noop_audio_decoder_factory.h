#pragma once

#include "pch.h"

using namespace webrtc;

namespace NoopAudio
{
class NoopAudioDecoderFactory final : public rtc::RefCountedObject<AudioDecoderFactory>
{
    // Inherited via RefCountedObject
    virtual std::vector<AudioCodecSpec> GetSupportedDecoders() override;
    virtual bool IsSupportedDecoder(const SdpAudioFormat &format) override;
    virtual std::unique_ptr<AudioDecoder> MakeAudioDecoder(
        const SdpAudioFormat &format,
        absl::optional<AudioCodecPairId> codec_pair_id) override;
};
} // namespace NoopAudio