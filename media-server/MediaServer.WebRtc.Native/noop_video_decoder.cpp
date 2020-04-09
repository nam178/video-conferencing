#include "pch.h"

#include "noop_video_frame_buffer.h"
#include "common_video/h264/h264_common.h"
#include "noop_video_decoder.h"

using namespace webrtc;

int32_t NoopVideo::Decoder::NoopVideoDecoder::InitDecode(const VideoCodec *, int32_t)
{
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Decoder::NoopVideoDecoder::Decode(const EncodedImage &input_image,
                                                     bool missing_frames,
                                                     const CodecSpecificInfo *,
                                                     int64_t)
{
    if(!_callback)
    {
        RTC_LOG(LS_WARNING) << "InitDecode() has been called, but a callback function "
                               "has not been set with RegisterDecodeCompleteCallback()";
        return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
    }
    if(!input_image._buffer || !input_image._length)
    {
        return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
    }

    // First make a copy of the encoded image
    // TODO need to modify EncodedImage to steal the buffer.
    auto input_image_copy = std::make_unique<EncodedImage>(input_image);
    input_image_copy->_buffer = static_cast<uint8_t*>(malloc(input_image._length));
    std::memcpy(input_image_copy->_buffer, input_image._buffer, input_image._length);

    // Then find the NAL units and
    // Create RTP fragments from those NAL units
    auto fragmentation_header = std::make_unique<RTPFragmentationHeader>();
    fragmentation_header->fragmentationVectorSize = 0;

    if(input_image_copy->_length > 0)
    {
        auto nalUnits = H264::FindNaluIndices(input_image_copy->_buffer, input_image_copy->_length);

        if(nalUnits.size() > 0) // can't be less than zero, but just tobe sure..
        {
            fragmentation_header->VerifyAndAllocateFragmentationHeader(nalUnits.size());
            for(int i = 0; i < nalUnits.size(); i++)
            {
                fragmentation_header->fragmentationOffset[i] = nalUnits[i].payload_start_offset;
                fragmentation_header->fragmentationLength[i] = nalUnits[i].payload_size;
            }
        }
    }

    // Construct NoopVideoFrameBuffer
    // Notes NoopVideoFrameBuffer takes ownership of the encodedImage
    // and will responsible for destroying the buffer
    auto buffer =
        new NoopVideoFrameBuffer(std::move(input_image_copy), std::move(fragmentation_header));

    auto result = VideoFrame::Builder()
                      .set_video_frame_buffer(rtc::scoped_refptr<NoopVideoFrameBuffer>(buffer))
                      .set_timestamp_rtp(input_image.Timestamp())
                      .set_ntp_time_ms(input_image.ntp_time_ms_)
                      .set_rotation(kVideoRotation_0)
                      .build();

    _callback->Decoded(result);
    _total++;
    if((_total % 30) == 0)
    {
        RTC_LOG(LS_VERBOSE) << "Total frames decoded: " << _total;
    }

    // Request keyframe every 30 frames
    _propagation_cnt++;
    if(_propagation_cnt >= 30)
    {
        _propagation_cnt = 0;
        return WEBRTC_VIDEO_CODEC_ERROR;
    }
    // Request key frame when we reach the threashold
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Decoder::NoopVideoDecoder::RegisterDecodeCompleteCallback(
    DecodedImageCallback *callback)
{
    _callback = callback;
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Decoder::NoopVideoDecoder::Release()
{
    // TODO
    return WEBRTC_VIDEO_CODEC_OK;
}
