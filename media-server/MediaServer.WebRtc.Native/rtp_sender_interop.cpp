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
    return rtp_sender->GetTrack();
}

void CONVENTION RtpSenderSetTrack(Shim::RtpSender *rtp_sender, Shim::MediaStreamTrack *track)
{
    rtp_sender->SetTrack(track);
}

void CONVENTION RtpSenderSetStreamId(Shim::RtpSender *rtp_sender, const char *stream_id)
{
    rtp_sender->SetStreamId(stream_id);
}

const char *CONVENTION RtpSenderGetStreamId(Shim::RtpSender *rtp_sender)
{
    return rtp_sender->GetStreamId();
}
