#pragma once

#include "pch.h"

extern "C"
{
    // Pointer to webrtc::RtpReceiverInterface
    using RtpReceiverInterfacePtr = void *;
    // Pointer to Wrappers::RtpReceiver
    using RtpReceiverWrapperPtr = void *;

    EXPORT RtpReceiverInterfacePtr CONVENTION
    RtpReceiverGetRtpReceiverInterface(RtpReceiverWrapperPtr rtp_receiver_ptr);

    EXPORT void CONVENTION RtpReceiverDestroy(RtpReceiverWrapperPtr rtp_receiver_ptr);
}