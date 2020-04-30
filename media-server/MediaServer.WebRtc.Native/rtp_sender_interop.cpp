#include "rtp_sender_interop.h"

void CONVENTION RtpSenderDestroy(Wrappers::RtpSender *rtp_sender)
{
    if(rtp_sender)
    {
        delete rtp_sender;
    }
}
