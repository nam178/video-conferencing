#include "pch.h"

#include "video_track.h"

Wrappers::VideoTrack::VideoTrack(
    rtc::scoped_refptr<webrtc::VideoTrackInterface> &&video_track_interface)
    : _video_track_interface(video_track_interface)
{
}

webrtc::MediaStreamTrackInterface *Wrappers::VideoTrack::GetMediaStreamTrack() const
{
    return _video_track_interface.get();
}
