#pragma once

#include "pch.h"

#include "media_track.h"

namespace Wrappers
{
class AudioTrack : public MediaTrack
{
  public:
    AudioTrack(rtc::scoped_refptr<webrtc::AudioTrackInterface> &&audio_track_interface);

  private:
    rtc::scoped_refptr<webrtc::AudioTrackInterface> _audio_track_interface;
};
} // namespace Wrappers