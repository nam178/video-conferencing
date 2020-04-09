#pragma once

#include "pch.h"

#include "api/video_codecs/video_encoder_factory.h"

using namespace webrtc;

namespace NoopVideo::Encoder
{
class NoopVideoEncoderFactory : public VideoEncoderFactory
{
    // Inherited via VideoEncoderFactory
    virtual std::vector<SdpVideoFormat> GetSupportedFormats() const override;
    virtual CodecInfo QueryVideoEncoder(const SdpVideoFormat &format) const override;
    virtual std::unique_ptr<VideoEncoder> CreateVideoEncoder(const SdpVideoFormat &format) override;
};
}
