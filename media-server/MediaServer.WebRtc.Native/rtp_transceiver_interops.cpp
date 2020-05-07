#include "pch.h"

#include "rtp_transceiver_interops.h"

void CONVENTION RtpTransceiverDestroy(Shim::RtpTransceiver *transceiver)
{
    if(transceiver)
    {
        delete transceiver;
    }
}

Shim::RtpSender *CONVENTION RtpTransceiverGetSender(Shim::RtpTransceiver *transceiver)
{
    return transceiver->Sender();
}

Shim::RtpReceiver *CONVENTION RtpTransceiverGetReceiver(Shim::RtpTransceiver *transceiver)
{
    return transceiver->Receiver();
}
