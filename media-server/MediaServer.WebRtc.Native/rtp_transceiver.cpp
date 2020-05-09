#include "pch.h"

#include "rtp_transceiver.h"

Shim::RtpTransceiver::RtpTransceiver(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver)
    : _transceiver(std::move(transceiver))
{

    _receiver.reset(new Shim::RtpReceiver(_transceiver->receiver()));
    _sender.reset(new Shim::RtpSender(_transceiver->sender()));
}

Shim::RtpReceiver *Shim::RtpTransceiver::Receiver()
{
    return _receiver.get();
}

Shim::RtpSender *Shim::RtpTransceiver::Sender()
{
    return _sender.get();
}
