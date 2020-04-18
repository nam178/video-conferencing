#include "pch.h"

#include "set_remote_session_description_observer.h"

using namespace webrtc;

Wrappers::SetRemoteSessionDescriptionObserver::SetRemoteSessionDescriptionObserver(
    std::function<void(RTCError)> &&callback_lambda)
    : _callback_lambda(callback_lambda)
{
}

void Wrappers::SetRemoteSessionDescriptionObserver::OnSetRemoteDescriptionComplete(RTCError error)
{
    _callback_lambda(std::move(error));
}
