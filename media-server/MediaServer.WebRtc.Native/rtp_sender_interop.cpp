#include "pch.h"

#include "rtp_sender_interop.h"

void CONVENTION RtpSenderDestroy(Shim::RtpSender *rtp_sender)
{
    if(rtp_sender)
    {
        delete rtp_sender;
    }
}

Shim::MediaStreamTrack *CONVENTION RtpSenderGetTrack(Shim::RtpSender *rtp_sender)
{
    rtp_sender->GetTrack();
}

void CONVENTION RtpSenderSetTrack(Shim::RtpSender *rtp_sender, Shim::MediaStreamTrack *track)
{
    rtp_sender->SetTrack(track);
}
