#include "pch.h"

#include "audio_track.h"

Wrappers::AudioTrack::AudioTrack(
    rtc::scoped_refptr<webrtc::AudioTrackInterface> &&audio_track_interface)
    : _audio_track_interface(audio_track_interface)
{
}
