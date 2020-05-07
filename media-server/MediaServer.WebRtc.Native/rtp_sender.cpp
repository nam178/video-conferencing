#include "pch.h"

#include "rtp_sender.h"

Shim::RtpSender::RtpSender(rtc::scoped_refptr<webrtc::RtpSenderInterface> native)
    : _native(native)
{
}

webrtc::RtpSenderInterface *Shim::RtpSender::Native()
{
    return _native.get();
}
