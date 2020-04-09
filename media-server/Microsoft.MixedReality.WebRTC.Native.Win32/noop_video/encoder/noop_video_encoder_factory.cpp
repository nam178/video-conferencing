#include "pch.h"

#include "../noop_video_helper.h"
#include "media/base/h264_profile_level_id.h"
#include "modules/video_coding/codecs/h264/include/h264.h"
#include "noop_video_encoder.h"
#include "noop_video_encoder_factory.h"

using namespace webrtc;

namespace NoopVideo::Encoder
{
std::vector<SdpVideoFormat> NoopVideoEncoderFactory::GetSupportedFormats() const
{
    return
    {
        // SdpVideoFormat(cricket::kVp8CodecName),
        // CreateH264Format(H264::kProfileBaseline, H264::kLevel3_1, "1"),
        // CreateH264Format(H264::kProfileBaseline, H264::kLevel3_1, "0"),
        CreateH264Format(H264::kProfileConstrainedBaseline, H264::kLevel3_1, "1"),
        // CreateH264Format(H264::kProfileConstrainedBaseline, H264::kLevel3_1, "0")
    };
}

VideoEncoderFactory::CodecInfo NoopVideoEncoderFactory::QueryVideoEncoder(
    const SdpVideoFormat &format) const
{
    CodecInfo info;
    info.is_hardware_accelerated = false;
    info.has_internal_source = false;
    return info;
}

std::unique_ptr<VideoEncoder> NoopVideoEncoderFactory::CreateVideoEncoder(
    const SdpVideoFormat &format)
{
    return std::make_unique<NoopVideoEncoder>();
}
} // namespace NoopVideo::Encoder
