// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include <mutex>

#include "api/video/video_frame.h"
#include "api/video/video_sink_interface.h"

#include "callback.h"
#include "video_frame.h"

#include "rtc_base/memory/aligned_malloc.h"

namespace Microsoft::MixedReality::WebRTC
{
using VideoFrameCallback = Callback<const webrtc::VideoFrame &>;

class VideoFrameObserver : public rtc::VideoSinkInterface<webrtc::VideoFrame>
{
  public:
    VideoFrameObserver(const VideoFrameCallback& callback);
    VideoFrameObserver();

  protected:
    // VideoSinkInterface interface
    void OnFrame(const webrtc::VideoFrame &frame) noexcept override;

  private:
    VideoFrameCallback _callback;
};

} // namespace Microsoft::MixedReality::WebRTC
