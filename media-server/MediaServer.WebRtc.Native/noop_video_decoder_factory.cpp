#include "pch.h"

#include "modules/video_coding/codecs/h264/include/h264.h"
#include "noop_video_decoder.h"
#include "noop_video_decoder_factory.h"
#include "noop_video_helper.h"

using namespace webrtc;

namespace NoopVideo::Decoder
{
std::vector<SdpVideoFormat> NoopVideoDecoderFactory::GetSupportedFormats() const
{
    return {
        // SdpVideoFormat(cricket::kVp8CodecName),
        // CreateH264Format(H264::kProfileBaseline, H264::kLevel3_1, "1"),
        // CreateH264Format(H264::kProfileBaseline, H264::kLevel3_1, "0"),
        CreateH264Format(H264::kProfileConstrainedBaseline, H264::kLevel3_1, "1"),
        // CreateH264Format(H264::kProfileConstrainedBaseline, H264::kLevel3_1, "0")
    };
}

std::unique_ptr<VideoDecoder> NoopVideoDecoderFactory::CreateVideoDecoder(
    const SdpVideoFormat &format)
{
    return std::make_unique<NoopVideoDecoder>();
}
} // namespace NoopVideo::Decoder
