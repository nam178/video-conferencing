#pragma once

#include "pch.h";

extern "C"
{
    // Add a sink to the provided transceiver's receiver
    // Notes
    // Must be invoked on worker thread
    EXPORT void CONVENTION
    RtpTransceiverInterfaceReceiverAddOrUpdateSink(void *rtp_transceiver_interface,
                                                   void *audio_or_video_sink);
}