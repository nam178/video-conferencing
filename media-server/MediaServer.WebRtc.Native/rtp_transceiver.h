#pragma once

#include "pch.h"

#include "rtp_receiver.h"
#include "rtp_sender.h"

namespace Shim
{

enum class RtpTransceiverDirection : int32_t
{
    SendRecv = 0,
    SendOnly = 1,
    RecvOnly = 2,
    Inactive = 3,
    Stopped = 4,
};

static_assert((int)RtpTransceiverDirection::SendRecv ==
              (int)webrtc::RtpTransceiverDirection::kSendRecv);
static_assert((int)RtpTransceiverDirection::SendOnly ==
              (int)webrtc::RtpTransceiverDirection::kSendOnly);
static_assert((int)RtpTransceiverDirection::RecvOnly ==
              (int)webrtc::RtpTransceiverDirection::kRecvOnly);
static_assert((int)RtpTransceiverDirection::Inactive ==
              (int)webrtc::RtpTransceiverDirection::kInactive);
static_assert((int)RtpTransceiverDirection::Stopped ==
              (int)webrtc::RtpTransceiverDirection::kStopped);

class RtpTransceiver
{

  public:
    RtpTransceiver(rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver);

    const char *Mid();
    RtpTransceiverDirection Direction() const;
    void SetDirection(RtpTransceiverDirection direction);
    bool CurrentDirection(RtpTransceiverDirection &direction) const;
    Shim::RtpReceiver *Receiver();
    Shim::RtpSender *Sender();
    cricket::MediaType MediaKind();

  private:
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> _transceiver;
    Shim::RtpReceiver *_receiver{}; // does not own, the managed code owns this
    Shim::RtpSender *_sender{};     // does not own, the managed code owns this
    std::string _mid{};
};
} // namespace Shim
