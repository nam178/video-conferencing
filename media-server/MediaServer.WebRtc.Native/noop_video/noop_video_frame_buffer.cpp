#pragma once
#include "pch.h"

#include "noop_video_frame_buffer.h"

namespace NoopVideo
{
NoopVideoFrameBuffer::NoopVideoFrameBuffer(
    std::unique_ptr<webrtc::EncodedImage> &&original,
    std::unique_ptr<webrtc::RTPFragmentationHeader> &&frag_header)
    : _original(std::move(original)), _frag_header(std::move(frag_header))
{
}

NoopVideoFrameBuffer::~NoopVideoFrameBuffer()
{
    free(_original->_buffer);
}

webrtc::VideoFrameBuffer::Type NoopVideoFrameBuffer::type() const
{
    return webrtc::VideoFrameBuffer::Type::kNative;
}
} // namespace NoopVideo
