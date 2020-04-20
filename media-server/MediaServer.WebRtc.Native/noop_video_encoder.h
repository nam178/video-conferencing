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
                           const CodecSpecificInfo *codec_specific_info,
                          const std::vector<FrameType> *frame_types) override;
    virtual bool SupportsNativeHandle() const;
    virtual int32_t SetRateAllocation(const VideoBitrateAllocation &allocation,
                                      uint32_t framerate) override;

  private:
    EncodedImageCallback *_callback;
    static CodecSpecificInfo _codec_specificInfo;

    // Inherited via VideoEncoder
    virtual int32_t SetChannelParameters(uint32_t packet_loss, int64_t rtt) override;
};
} // namespace NoopVideo::Encoder
