#pragma once

#include "pch.h"

#include "modules/video_coding/include/video_codec_interface.h"

namespace NoopVideo::Decoder
{
class NoopVideoDecoder : public webrtc::VideoDecoder
{
    // Inherited via VideoDecoder
    virtual int32_t InitDecode(const webrtc::VideoCodec *codec_settings,
                               int32_t number_of_cores) override;
    virtual int32_t Decode(const webrtc::EncodedImage &input_image,
                           bool missing_frames,
                           const webrtc::CodecSpecificInfo *codec_specific_info,
                           int64_t render_time_ms) override;
    virtual int32_t RegisterDecodeCompleteCallback(webrtc::DecodedImageCallback *callback) override;
    virtual int32_t Release() override;

  private:
    webrtc::DecodedImageCallback *_callback;
    int32_t _propagation_cnt = 0;
    uint32_t _total = 0;
};
} // namespace NoopVideo::Decoder
