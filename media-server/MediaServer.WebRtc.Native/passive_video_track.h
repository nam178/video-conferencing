#pragma once

#include "pch.h"

#include "passive_video_track_source.h"

using namespace rtc;
using namespace webrtc;

namespace Video
{
class PassiveVideoTrack
{
  public:
    PassiveVideoTrack(scoped_refptr<VideoTrackInterface> video_track);

    // Get the native webrtc video track,
    // used to add this track into the peer connection
    scoped_refptr<VideoTrackInterface> VideoTrack();

  private:
    scoped_refptr<VideoTrackInterface> _video_track;
};
} // namespace Video
