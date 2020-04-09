#include "pch.h"

#include "../noop_video_frame_buffer.h"
#include "noop_video_encoder.h"

webrtc::CodecSpecificInfo getCodecSpecificInfo()
{
    CodecSpecificInfo result = CodecSpecificInfo();

    result.codecType = webrtc::VideoCodecType::kVideoCodecH264;
    result.codecSpecific.H264 = webrtc::CodecSpecificInfoH264();

    webrtc::CodecSpecificInfoH264 tmp = webrtc::CodecSpecificInfoH264();
    tmp.packetization_mode = webrtc::H264PacketizationMode::NonInterleaved;
    result.codecSpecific.H264 = tmp;
    return result;
}

CodecSpecificInfo NoopVideo::Encoder::NoopVideoEncoder::_codec_specificInfo =
    getCodecSpecificInfo();

int32_t NoopVideo::Encoder::NoopVideoEncoder::InitEncode(const VideoCodec *codec_settings,
                                                         int32_t number_of_cores,
                                                         size_t max_payload_size)
{
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Encoder::NoopVideoEncoder::RegisterEncodeCompleteCallback(
    EncodedImageCallback *callback)
{
    _callback = callback;
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Encoder::NoopVideoEncoder::Encode(const VideoFrame &frame,
                                                     const webrtc::CodecSpecificInfo *,
                                                     const std::vector<FrameType> *frame_types)
{
    if(!_callback)
    {
        RTC_LOG(LS_WARNING) << "InitEncode() has been called, but a callback function "
                            << "has not been set with RegisterEncodeCompleteCallback()";
        return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
    }

    auto noopVideoFrameBuffer =
        static_cast<NoopVideoFrameBuffer *>(frame.video_frame_buffer().get());

  _callback->OnEncodedImage(*noopVideoFrameBuffer->_original.get(),
                              &_codec_specificInfo,
                              noopVideoFrameBuffer->_frag_header.get());

    _total++;
    if((_total % 30) == 0)
    {
        RTC_LOG(LS_VERBOSE) << "Total frames encoded: " << _total;
    }

    return WEBRTC_VIDEO_CODEC_OK;
}

bool NoopVideo::Encoder::NoopVideoEncoder::SupportsNativeHandle() const
{
    return true;
}

int32_t NoopVideo::Encoder::NoopVideoEncoder::SetRateAllocation(
    const VideoBitrateAllocation &allocation,
    uint32_t framerate)
{
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Encoder::NoopVideoEncoder::SetChannelParameters(uint32_t packet_loss,
                                                                   int64_t rtt)
{
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Encoder::NoopVideoEncoder::Release()
{
    return WEBRTC_VIDEO_CODEC_OK;
}

