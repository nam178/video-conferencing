#pragma once

#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_sender.h"
#include "rtp_transceiver.h"

extern "C"
{
    EXPORT void CONVENTION RtpTransceiverDestroy(Wrappers::RtpTransceiver *transceiver);

    EXPORT Wrappers::RtpSender *CONVENTION
    RtpTransceiverGetSender(Wrappers::RtpTransceiver *transceiver);

    EXPORT Wrappers::RtpReceiver *CONVENTION
    RtpTransceiverGetReceiver(Wrappers::RtpTransceiver *transceiver);
}