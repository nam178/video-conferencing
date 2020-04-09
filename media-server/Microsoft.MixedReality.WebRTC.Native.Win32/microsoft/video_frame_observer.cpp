// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license
// information.

#include "pch.h"

#include "video_frame_observer.h"

namespace
{

// Aligning pointer to 64 bytes for improved performance, e.g. use SIMD.
constexpr int kBufferAlignment = 64;

} // namespace

namespace Microsoft::MixedReality::WebRTC
{

VideoFrameObserver::VideoFrameObserver(const VideoFrameCallback &callback) : _callback(callback)
{
}

VideoFrameObserver::VideoFrameObserver()
{
}

void VideoFrameObserver::OnFrame(const webrtc::VideoFrame &frame) noexcept
{
    _callback(frame);
}

} // namespace Microsoft::MixedReality::WebRTC
