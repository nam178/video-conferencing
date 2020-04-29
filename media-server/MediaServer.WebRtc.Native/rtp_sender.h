#pragma once

#include "pch.h"

namespace Wrappers
{
class RtpSender
{
  public:
    RtpSender(rtc::scoped_refptr<webrtc::RtpSenderInterface> native);

  private:
    rtc::scoped_refptr<webrtc::RtpSenderInterface> _native;
};
} // namespace Wrappers