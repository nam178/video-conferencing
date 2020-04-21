#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_receiver_interop.h"

RtpReceiverInterfacePtr CONVENTION
RtpReceiverGetRtpReceiverInterface(RtpReceiverWrapperPtr rtp_receiver_ptr)
{
    return StaticCastOrThrow<Wrappers::RtpReceiver>(rtp_receiver_ptr)->GetRtpReceiverInterface();
}

void CONVENTION RtpReceiverDestroy(RtpReceiverWrapperPtr rtp_receiver_ptr)
{
    if(rtp_receiver_ptr)
    {
        delete static_cast<Wrappers::RtpReceiver *>(rtp_receiver_ptr);
    }
}
