#pragma once

#include "pch.h"

#include "media_stream_track.h"

namespace Shim
{
class AudioTrack : public MediaStreamTrack
{
  public:
    AudioTrack(rtc::scoped_refptr<webrtc::AudioTrackInterface> &&audio_track_interface);

  private:
    rtc::scoped_refptr<webrtc::AudioTrackInterface> _audio_track_interface;

    // Inherited via MediaStreamTrack
    virtual webrtc::MediaStreamTrackInterface *GetMediaStreamTrack() const override;
};
} // namespace Shim