#include "pch.h"

#include "rtp_transceiver.h"

Shim::RtpTransceiver::RtpTransceiver(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> &&transceiver)
    : _transceiver(std::move(transceiver))
{

    _receiver = new Shim::RtpReceiver(_transceiver->receiver());
    _sender = new Shim::RtpSender(_transceiver->sender());
}

const char *Shim::RtpTransceiver::Mid()
{
    // Copy the mid locally so we can ensure it is null-terminated and
    // to return to unmanaged_transceiver code
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

Shim::RtpTransceiverDirection Shim::RtpTransceiver::Direction() const
{
    return (RtpTransceiverDirection)_transceiver->direction();
}

void Shim::RtpTransceiver::SetDirection(RtpTransceiverDirection direction)
{
    _transceiver->SetDirection((webrtc::RtpTransceiverDirection)direction);
}

bool Shim::RtpTransceiver::CurrentDirection(RtpTransceiverDirection &direction) const
{
    auto tmp = _transceiver->current_direction();
    if(tmp.has_value())
    {
        direction = (RtpTransceiverDirection)tmp.value();
        return true;
    }
    return false;
}

Shim::RtpReceiver *Shim::RtpTransceiver::Receiver()
{
    return _receiver;
}

Shim::RtpSender *Shim::RtpTransceiver::Sender()
{
    return _sender;
}

cricket::MediaType Shim::RtpTransceiver::MediaKind()
{
    return _transceiver->media_type();
}
