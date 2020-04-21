#include "pch.h"

#include "rtp_receiver.h"

Wrappers::RtpReceiver::RtpReceiver(rtc::scoped_refptr<webrtc::RtpReceiverInterface> &&rtp_receiver)
    : _rtp_receiver(rtp_receiver)
{
}

Wrappers::RtpReceiver::~RtpReceiver()
{
    // Make sure this gets called
    ;
}

webrtc::RtpReceiverInterface *Wrappers::RtpReceiver::GetRtpReceiverInterface() const
{
    return _rtp_receiver.get();
}
