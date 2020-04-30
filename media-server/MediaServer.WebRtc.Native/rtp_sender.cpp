#include "pch.h"

#include "rtp_sender.h"

Wrappers::RtpSender::RtpSender(rtc::scoped_refptr<webrtc::RtpSenderInterface> native)
    : _native(native)
{
}

webrtc::RtpSenderInterface *Wrappers::RtpSender::Native()
{
    return _native.get();
}
