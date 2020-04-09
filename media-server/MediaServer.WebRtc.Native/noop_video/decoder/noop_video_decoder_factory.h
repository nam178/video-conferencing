#pragma once

#include "pch.h"

#include "api/video_codecs/video_decoder_factory.h"

namespace NoopVideo::Decoder
{
class NoopVideoDecoderFactory : public webrtc::VideoDecoderFactory
{
  public:
    std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
    std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(
        const webrtc::SdpVideoFormat &format) override;
};
} // namespace NoopVideo

