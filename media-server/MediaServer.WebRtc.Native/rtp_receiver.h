#pragma once

#include "pch.h"

namespace Wrappers
{
class RtpReceiver
{
  public:
    RtpReceiver(rtc::scoped_refptr<webrtc::RtpReceiverInterface>&& rtp_receiver);
    ~RtpReceiver();
    webrtc::RtpReceiverInterface *GetRtpReceiverInterface() const;

  private:
    rtc::scoped_refptr<webrtc::RtpReceiverInterface> _rtp_receiver;
};
} // namespace Wrappers
