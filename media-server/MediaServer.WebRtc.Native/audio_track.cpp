#include "pch.h"

#include "audio_track.h"

Shim::AudioTrack::AudioTrack(
    rtc::scoped_refptr<webrtc::AudioTrackInterface> &&audio_track_interface)
    : _audio_track_interface(audio_track_interface)
{
}

webrtc::MediaStreamTrackInterface *Shim::AudioTrack::GetMediaStreamTrack() const
{
    return _audio_track_interface.get();
}
