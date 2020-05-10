#include "pch.h"

#include "rtp_sender.h"

void ThrowTrackNotTheSame()
{
    RTC_LOG(LS_ERROR) << "Library track and Shim::RtpSender tracks some how not the same.";
}

Shim::RtpSender::RtpSender(rtc::scoped_refptr<webrtc::RtpSenderInterface> native) : _native(native)
{
}

webrtc::RtpSenderInterface *Shim::RtpSender::Native()
{
    return _native.get();
}

// Not thread safe
// Should be called on signalling thread
Shim::MediaStreamTrack *Shim::RtpSender::GetTrack()
{
    // We don't allow clients to add tracks via PeerConnection AddTrack(),
    // instead all the addition/removal of tracks are done thru this instance.
    // That means the library's track and this instance's tracks are aways the same.
    auto track = _native->track();
    if(track && _track)
    {
        if(track.get() != _track->GetMediaStreamTrack())
        {
            ThrowTrackNotTheSame();
        }
    }
    else if(track || _track)
    {
        ThrowTrackNotTheSame();
    }
    // Passed our test - just return the shim track we have
    return _track;
}

// Not thread safe
// Should be called on signalling thread
void Shim::RtpSender::SetTrack(Shim::MediaStreamTrack *track)
{
    auto lib_webrtc_track = track ? track->GetMediaStreamTrack() : nullptr;

    if(false == _native->SetTrack(lib_webrtc_track))
    {
        RTC_LOG(LS_ERROR) << "Failed setting track";
        throw std::runtime_error("Failed setting track");
    }
    _track = track;
}

void Shim::RtpSender::SetStreamId(const char *stream_id)
{
    std::vector<std::string> stream_ids{};
    stream_ids.push_back(stream_id);
    _native->SetStreams(stream_ids);
}
