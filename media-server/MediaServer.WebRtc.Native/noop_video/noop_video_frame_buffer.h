#pragma once
#include "pch.h"

#include "common_video\include\video_frame.h"
#include "modules/video_coding/include/video_codec_interface.h"

namespace NoopVideo
{
const int NOOP_VIDEO_FRAME_SIZE = 320;

class NoopVideoFrameBuffer : public rtc::RefCountedObject<webrtc::VideoFrameBuffer>
{
  public:
    // Constructors, Destructors
    NoopVideoFrameBuffer(std::unique_ptr<webrtc::EncodedImage> &&original,
                         std::unique_ptr<webrtc::RTPFragmentationHeader> &&frag_header);
    ~NoopVideoFrameBuffer() override;

    // Fields
    std::unique_ptr<webrtc::EncodedImage> _original;
    std::unique_ptr<webrtc::RTPFragmentationHeader> _frag_header;

    // Override VideoFrameBuffer
    Type type() const override;
    int width() const override
    {
        return NOOP_VIDEO_FRAME_SIZE;
    }
    int height() const override
    {
        return NOOP_VIDEO_FRAME_SIZE;
    }

    rtc::scoped_refptr<webrtc::I420BufferInterface> ToI420() override
    {
        throw new NotSupportedException();
    }
};
} // namespace NoopVideo
