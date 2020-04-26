#pragma once

#include "pch.h"

#include "passive_video_track_source.h"

namespace Video
{
// Converts a Video::PassiveVideoTrackSource into a rtc::VideoSinkInterface<webrtc::VideoFrame>
class VideoSinkAdapter : public rtc::VideoSinkInterface<webrtc::VideoFrame>
{
  public:
    // This adapter doesn't take ownership of the provided video_track_source;
    VideoSinkAdapter(Video::PassiveVideoTrackSource *video_track_source);

    // Initialise this adapter,
    // will be executed on the provided worker thread
    void Initialise(rtc::Thread *worker_thread);

    // Inherited via VideoSinkInterface
    virtual void OnFrame(const webrtc::VideoFrame &frame) override;

  private:
    Video::PassiveVideoTrackSource *_video_track_source;
};
} // namespace Video