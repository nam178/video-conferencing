#pragma once

#include "pch.h"

extern "C"
{
    // Pointer to webrtc::RtpReceiverInterface
    using RtpReceiverInterfacePtr = void *;
    // Pointer to Wrappers::RtpReceiver
    using RtpReceiverWrapperPtr = void *;
    // Pointer to either Wrappers::AudioTrack or Wrappers::VideoTrack
    using TrackWrapeprPtr = void *;

    EXPORT RtpReceiverInterfacePtr CONVENTION
    RtpReceiverGetRtpReceiverInterface(RtpReceiverWrapperPtr rtp_receiver_ptr);

    EXPORT void CONVENTION RtpReceiverDestroy(RtpReceiverWrapperPtr rtp_receiver_ptr);

    // Get the track associated with this RtpReceiver.
    // Note that the managed code will take ownership.
    EXPORT TrackWrapeprPtr CONVENTION RtpReceiverGetTrack(RtpReceiverWrapperPtr rtp_receiver_ptr);
}