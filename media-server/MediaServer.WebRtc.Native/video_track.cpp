#include "pch.h"

#include "video_track.h"

Wrappers::VideoTrack::VideoTrack(
    rtc::scoped_refptr<webrtc::VideoTrackInterface> &&video_track_interface)
    : _video_track_interface(video_track_interface)
{
    if(!video_track_interface)
    {
        RTC_LOG(LS_ERROR) << "video_track_interface is nullptr";
        throw new std::runtime_error("video_track_interface is nullptr");
    }
}

void Wrappers::VideoTrack::AddSink(rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink)
{
    _video_track_interface->AddOrUpdateSink(video_sink, rtc::VideoSinkWants{});
}

void Wrappers::VideoTrack::RemoveSink(rtc::VideoSinkInterface<webrtc::VideoFrame> *video_sink)
{
    _video_track_interface->RemoveSink(video_sink);
}

webrtc::MediaStreamTrackInterface *Wrappers::VideoTrack::GetMediaStreamTrack() const
{
    return _video_track_interface.get();
}
