#pragma once

#include "pch.h"

#include "media_stream_track.h"

namespace Shim
{
class VideoTrack : public MediaStreamTrack
{
  public:
    VideoTrack(rtc::scoped_refptr<webrtc::VideoTrackInterface> &&video_track_interface);

    // Add sink. Automatically proxied to worker thread by lib WebRTC
    void AddSink(rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink);
    // Remove sink. Automatically proxied to worker thread by lib WebRTC
    void RemoveSink(rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink);

  private:
    rtc::scoped_refptr<webrtc::VideoTrackInterface> _video_track_interface;

    // Inherited via MediaStreamTrack
    virtual webrtc::MediaStreamTrackInterface *GetMediaStreamTrack() const override;
};
} // namespace Shim