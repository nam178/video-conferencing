#pragma once

#include "pch.h"

#include "passive_video_track_source.h"

namespace MediaSources
{
// Converts a MediaSources::PassiveVideoTrackSource into a rtc::VideoSinkInterface<webrtc::VideoFrame>
class VideoSinkAdapter : public rtc::VideoSinkInterface<webrtc::VideoFrame>
{
  public:
    // This adapter doesn't take ownership of the provided video_track_source;
    VideoSinkAdapter(MediaSources::PassiveVideoTrackSource *video_track_source);

    // Inherited via VideoSinkInterface
    virtual void OnFrame(const webrtc::VideoFrame &frame) override;

  private:
    MediaSources::PassiveVideoTrackSource *_video_track_source;
};
} // namespace MediaSources