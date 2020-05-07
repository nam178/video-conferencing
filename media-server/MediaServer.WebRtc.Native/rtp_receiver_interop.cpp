#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_receiver_interop.h"

webrtc::RtpReceiverInterface *CONVENTION
RtpReceiverGetRtpReceiverInterface(Shim::RtpReceiver *rtp_receiver_ptr)
{
    return rtp_receiver_ptr->GetRtpReceiverInterface();
}

void CONVENTION RtpReceiverDestroy(Shim::RtpReceiver *rtp_receiver_ptr)
{
    if(rtp_receiver_ptr)
    {
        delete static_cast<Shim::RtpReceiver *>(rtp_receiver_ptr);
    }
}

Shim::MediaStreamTrack *CONVENTION RtpReceiverGetTrack(Shim::RtpReceiver *rtp_receiver_ptr)
{
    return rtp_receiver_ptr->GetTrack().release();
}