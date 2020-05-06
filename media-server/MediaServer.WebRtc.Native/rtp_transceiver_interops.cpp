#include "pch.h"

#include "rtp_transceiver_interops.h"

void CONVENTION RtpTransceiverDestroy(Wrappers::RtpTransceiver *transceiver)
{
    if(transceiver)
    {
        delete transceiver;
    }
}

Wrappers::RtpSender *CONVENTION RtpTransceiverGetSender(Wrappers::RtpTransceiver *transceiver)
{
    return transceiver->Sender();
}

Wrappers::RtpReceiver *CONVENTION RtpTransceiverGetReceiver(Wrappers::RtpTransceiver *transceiver)
{
    return transceiver->Receiver();
}
