#pragma once

#include "pch.h"

extern "C"
{
    EXPORT webrtc::RtpReceiverInterface *CONVENTION
    RtpReceiverGetRtpReceiverInterface(Shim::RtpReceiver *rtp_receiver_ptr);

    EXPORT void CONVENTION RtpReceiverDestroy(Shim::RtpReceiver *rtp_receiver_ptr);

    // Get the track associated with this RtpReceiver.
    // Note that the managed code will take ownership.
    EXPORT Shim::MediaStreamTrack *CONVENTION
    RtpReceiverGetTrack(Shim::RtpReceiver *rtp_receiver_ptr);
}