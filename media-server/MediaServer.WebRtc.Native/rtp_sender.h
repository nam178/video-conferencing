#pragma once

#include "pch.h"

namespace Shim
{
class RtpSender
{
  public:
    RtpSender(rtc::scoped_refptr<webrtc::RtpSenderInterface> native);

    webrtc::RtpSenderInterface *Native();

  private:
    rtc::scoped_refptr<webrtc::RtpSenderInterface> _native;
};
} // namespace Shim