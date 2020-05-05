#pragma once

#include "pch.h"

#include "modules\include\module_common_types.h"
#include "modules\video_coding\include\video_codec_interface.h"

using namespace webrtc;

namespace NoopVideo::Encoder
{
class NoopVideoEncoder : public VideoEncoder
{
  public:
    // Inherited via VideoEncoder
    virtual int32_t InitEncode(const VideoCodec *codec_settings,
                               int32_t number_of_cores,
                               size_t max_payload_size) override;
    virtual int32_t RegisterEncodeCompleteCallback(EncodedImageCallback *callback) override;
    virtual int32_t Release() override;
    virtual int32_t Encode(const VideoFrame &frame,
                           const std::vector<VideoFrameType> *frame_types) override;
    virtual bool SupportsNativeHandle() const;
    virtual void SetRates(const RateControlParameters &parameters) override;
    virtual EncoderInfo GetEncoderInfo() const override
    {
        EncoderInfo result{};
        auto scaling_settings = VideoEncoder::ScalingSettings(VideoEncoder::ScalingSettings::kOff);
        result.has_trusted_rate_controller = true;
        result.scaling_settings = scaling_settings;
        return result;
    };

  private:
    EncodedImageCallback *_callback;
    static CodecSpecificInfo _codec_specificInfo;
};
} // namespace NoopVideo::Encoder
