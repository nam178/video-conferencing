#include "pch.h"

#include "rtp_sender_interop.h"

void CONVENTION RtpSenderDestroy(Shim::RtpSender *rtp_sender)
{
    if(rtp_sender)
    {
        delete rtp_sender;
    }
}
