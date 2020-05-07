#include "pch.h"

#include "audio_track.h";
#include "rtp_receiver.h"
#include "video_track.h"

Shim::RtpReceiver::RtpReceiver(rtc::scoped_refptr<webrtc::RtpReceiverInterface> &&rtp_receiver)
    : _rtp_receiver(rtp_receiver)
{
}

webrtc::RtpReceiverInterface *Shim::RtpReceiver::GetRtpReceiverInterface() const
{
    return _rtp_receiver.get();
}

std::unique_ptr<Shim::MediaStreamTrack> Shim::RtpReceiver::GetTrack()
{
    auto track = _rtp_receiver->track();
    if(track->kind() == webrtc::MediaStreamTrackInterface::kAudioKind)
    {
        return std::make_unique<Shim::AudioTrack>(
            rtc::scoped_refptr<webrtc::AudioTrackInterface>(
                static_cast<webrtc::AudioTrackInterface *>(track.release())));
    }
    else
    {
        return std::make_unique<Shim::VideoTrack>(
            rtc::scoped_refptr<webrtc::VideoTrackInterface>(
                static_cast<webrtc::VideoTrackInterface *>(track.release())));
    }
}
