#include "pch.h"

#include "rtp_transceiver.h"

Wrappers::RtpTransceiver::RtpTransceiver(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver)
    : _transceiver(std::move(transceiver))
{

    _receiver.reset(new Wrappers::RtpReceiver(transceiver->receiver()));
    _sender.reset(new Wrappers::RtpSender(transceiver->sender()));
}

Wrappers::RtpReceiver *Wrappers::RtpTransceiver::Receiver()
{
    return _receiver.get();
}

Wrappers::RtpSender *Wrappers::RtpTransceiver::Sender()
{
    return _sender.get();
}
