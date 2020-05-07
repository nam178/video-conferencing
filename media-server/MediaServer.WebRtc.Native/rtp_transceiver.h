#pragma once

#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_sender.h"

namespace Shim
{
class RtpTransceiver
{
  public:
    RtpTransceiver(rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver);

    const char *Mid()
    {
        // Copy the mid locally so we can ensure it is null-terminated and 
        // to return to unmanaged code
        auto tmp = _transceiver->mid();
        if(tmp.has_value())
        {
            _mid = tmp.value();
            Utils::StringHelper::EnsureNullTerminatedCString(_mid);
            return _mid.c_str();
        }
        else
        {
            return nullptr;
        }
    }

    Shim::RtpReceiver *Receiver();
    Shim::RtpSender *Sender();

  private:
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> _transceiver;
    std::unique_ptr<Shim::RtpReceiver> _receiver{};
    std::unique_ptr<Shim::RtpSender> _sender{};
    std::string _mid{};
};
} // namespace Shim
