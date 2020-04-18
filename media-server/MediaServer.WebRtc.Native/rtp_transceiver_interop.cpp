#include "pch.h"

#include "api/video/video_sink_interface.h"
#include "rtp_transceiver_interop.h"

void CONVENTION RtpTransceiverInterfaceReceiverAddOrUpdateSink(void *rtp_transceiver_interface,
                                                               void *audio_or_video_sink)
{
    // Cast
    auto rtp_transceiver =
        static_cast<webrtc::RtpTransceiverInterface *>(rtp_transceiver_interface);
    if(!rtp_transceiver)
    {
        throw new std::runtime_error("failed casting rtp_transceiver_interface");
    }

    // Get receiver
    auto rtp_receiver = rtp_transceiver->receiver();
    if(!rtp_receiver)
    {
        throw new std::runtime_error("failed getting receiver");
    }

    // Get track
    auto track = rtp_receiver->track();
    if(!track)
    {
        throw new std::runtime_error("receiver has no track");
    }

    // Add video or audio sink depending the type of this track
    const std::string &track_kind = track->kind();
    if(track_kind == webrtc::MediaStreamTrackInterface::kAudioKind)
    {
        static_cast<webrtc::AudioTrackInterface *>(track.get())
            ->AddSink(static_cast<webrtc::AudioTrackSinkInterface *>(audio_or_video_sink));
    }
    else if(track_kind == webrtc::MediaStreamTrackInterface::kVideoKind)
    {
        rtc::VideoSinkWants sink_wants{};
        sink_wants.rotation_applied = true; // no exposed API for caller to handle rotation

        static_cast<webrtc::VideoTrackInterface *>(track.get())
            ->AddOrUpdateSink(
                static_cast<rtc::VideoSinkInterface<webrtc::VideoFrame> *>(audio_or_video_sink),
                sink_wants);
    }
    else
    {
        throw new std::runtime_error("unknown track type " + track_kind);
    }
}
