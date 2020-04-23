#pragma once

#include "pch.h"

#include "media_stream_track.h"

namespace Wrappers
{
class VideoTrack : public MediaStreamTrack
{
  public:
    VideoTrack(rtc::scoped_refptr<webrtc::VideoTrackInterface> &&video_track_interface);

  private:
    rtc::scoped_refptr<webrtc::VideoTrackInterface> _video_track_interface;

    // Inherited via MediaStreamTrack
    virtual webrtc::MediaStreamTrackInterface *GetMediaStreamTrack() const override;
};
} // namespace Wrappers