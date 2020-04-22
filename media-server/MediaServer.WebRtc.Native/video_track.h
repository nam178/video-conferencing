#pragma once

#include "pch.h"

#include "media_track.h"

namespace Wrappers
{
class VideoTrack : public MediaTrack
{
  public:
    VideoTrack(rtc::scoped_refptr<webrtc::VideoTrackInterface> &&video_track_interface);

  private:
    rtc::scoped_refptr<webrtc::VideoTrackInterface> _video_track_interface;
};
} // namespace Wrappers