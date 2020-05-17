#include "pch.h"

#include "common_video/h264/h264_common.h"
#include "noop_video_decoder.h"
#include "noop_video_frame_buffer.h"

const int32_t KEYFRAME_REQUEST_INTERVAL = 5;

using namespace webrtc;

NoopVideo::Decoder::NoopVideoDecoder::NoopVideoDecoder()
    : _callback(nullptr),
      _last_keyframe_request(std::chrono::system_clock::now() -
                             std::chrono::seconds(KEYFRAME_REQUEST_INTERVAL))
{
}

int32_t NoopVideo::Decoder::NoopVideoDecoder::InitDecode(const VideoCodec *, int32_t)
{
    return WEBRTC_VIDEO_CODEC_OK;
}

int32_t NoopVideo::Decoder::NoopVideoDecoder::Decode(const EncodedImage &input_image,
                                                     bool missing_frames,
                                                     int64_t render_time_ms)
{
    if(!_callback)
    {
        RTC_LOG(LS_WARNING) << "InitDecode() has been called, but a callback function "
                               "has not been set with RegisterDecodeCompleteCallback()";
        return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
    }
    if(!input_image.data() || !input_image.size())
    {
        return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
    }

    // First make a copy of the encoded image
    // TODO need to modify EncodedImage to steal the buffer.
    uint8_t *buffer = static_cast<uint8_t *>(malloc(input_image.size()));
    std::memcpy(buffer, input_image.data(), input_image.size());

    auto input_image_copy = std::make_unique<EncodedImage>(input_image);
    input_image_copy->set_buffer(buffer, input_image.capacity());
    input_image_copy->set_size(input_image.size());

    // Then find the NAL units and
    // Create RTP fragments from those NAL units
    auto fragmentation_header = std::make_unique<RTPFragmentationHeader>();
    fragmentation_header->fragmentationVectorSize = 0;

    if(input_image_copy->data() > 0)
    {
        auto nalUnits = H264::FindNaluIndices(input_image_copy->data(), input_image_copy->size());

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

    auto result = VideoFrame::Builder()
                      .set_video_frame_buffer(
                          rtc::scoped_refptr<NoopVideoFrameBuffer>(new NoopVideoFrameBuffer(
                              std::move(input_image_copy), std::move(fragmentation_header))))
                      .set_timestamp_rtp(input_image.Timestamp())
                      .set_ntp_time_ms(input_image.ntp_time_ms_)
                      .set_rotation(kVideoRotation_0)
                      .build();

    _callback->Decoded(result);

    // Request keyframe every 5 seconds
    auto now = std::chrono::system_clock::now();
    std::chrono::duration<double> duration_since_last_iframe = now - _last_keyframe_request;
    if(duration_since_last_iframe.count() >= KEYFRAME_REQUEST_INTERVAL)
    {
        _last_keyframe_request = now;
        return WEBRTC_VIDEO_CODEC_OK_REQUEST_KEYFRAME;
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
