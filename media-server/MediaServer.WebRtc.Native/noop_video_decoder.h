#pragma once

#include "pch.h"

#include "modules/video_coding/include/video_codec_interface.h"

using namespace webrtc;

namespace NoopVideo::Decoder
{
class NoopVideoDecoder : public webrtc::VideoDecoder
{
  public:
    NoopVideoDecoder();

    // Inherited via VideoDecoder
    virtual int32_t InitDecode(const VideoCodec *codec_settings, int32_t number_of_cores) override;
    virtual int32_t Decode(const EncodedImage &input_image,
                           bool missing_frames,
                           int64_t render_time_ms) override;
    virtual int32_t RegisterDecodeCompleteCallback(webrtc::DecodedImageCallback *callback) override;
    virtual int32_t Release() override;

  private:
    webrtc::DecodedImageCallback *_callback;
    std::chrono::system_clock::time_point _last_keyframe_request;
};
} // namespace NoopVideo::Decoder
