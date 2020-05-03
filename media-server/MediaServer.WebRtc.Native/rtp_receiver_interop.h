#pragma once

#include "pch.h"

extern "C"
{
    EXPORT webrtc::RtpReceiverInterface *CONVENTION
    RtpReceiverGetRtpReceiverInterface(Wrappers::RtpReceiver *rtp_receiver_ptr);

    EXPORT void CONVENTION RtpReceiverDestroy(Wrappers::RtpReceiver *rtp_receiver_ptr);

    // Get the track associated with this RtpReceiver.
    // Note that the managed code will take ownership.
    EXPORT Wrappers::MediaStreamTrack *CONVENTION
    RtpReceiverGetTrack(Wrappers::RtpReceiver *rtp_receiver_ptr);
}