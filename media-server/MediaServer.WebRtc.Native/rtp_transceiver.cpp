#include "pch.h"

#include "rtp_transceiver.h"

Shim::RtpTransceiver::RtpTransceiver(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver)
    : _transceiver(std::move(transceiver))
{

    _receiver = new Shim::RtpReceiver(_transceiver->receiver());
    _sender = new Shim::RtpSender(_transceiver->sender());
}

Shim::RtpReceiver *Shim::RtpTransceiver::Receiver()
{
    return _receiver;
}

Shim::RtpSender *Shim::RtpTransceiver::Sender()
{
    return _sender;
}
