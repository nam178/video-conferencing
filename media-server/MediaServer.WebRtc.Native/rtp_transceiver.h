#pragma once

#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_sender.h"

namespace Wrappers
{
class RtpTransceiver
{
  public:
    RtpTransceiver(rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver);

    Wrappers::RtpReceiver *Receiver();
    Wrappers::RtpSender *Sender();

  private:
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> _transceiver;
    std::unique_ptr<Wrappers::RtpReceiver> _receiver{};
    std::unique_ptr<Wrappers::RtpSender> _sender{};
};
} // namespace Wrappers
