#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_receiver_interop.h"

webrtc::RtpReceiverInterface *CONVENTION
RtpReceiverGetRtpReceiverInterface(Wrappers::RtpReceiver *rtp_receiver_ptr)
{
    return rtp_receiver_ptr->GetRtpReceiverInterface();
}

void CONVENTION RtpReceiverDestroy(Wrappers::RtpReceiver *rtp_receiver_ptr)
{
    if(rtp_receiver_ptr)
    {
        delete static_cast<Wrappers::RtpReceiver *>(rtp_receiver_ptr);
    }
}

Wrappers::MediaStreamTrack *CONVENTION RtpReceiverGetTrack(Wrappers::RtpReceiver *rtp_receiver_ptr)
{
    return rtp_receiver_ptr->GetTrack().release();
}