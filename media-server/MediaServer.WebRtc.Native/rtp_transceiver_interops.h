#pragma once

#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_sender.h"
#include "rtp_transceiver.h"

extern "C"
{
    EXPORT void CONVENTION RtpTransceiverDestroy(Shim::RtpTransceiver *transceiver);

    EXPORT Shim::RtpSender *CONVENTION
    RtpTransceiverGetSender(Shim::RtpTransceiver *transceiver);

    EXPORT Shim::RtpReceiver *CONVENTION
    RtpTransceiverGetReceiver(Shim::RtpTransceiver *transceiver);
}