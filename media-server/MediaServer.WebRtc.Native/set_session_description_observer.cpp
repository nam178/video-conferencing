#include "pch.h"

#include "set_session_description_observer.h"

using namespace webrtc;

Wrappers::SetRemoteSessionDescriptionObserver::SetRemoteSessionDescriptionObserver(
    std::function<void(Success, ErrorMessage)> &&callback_lambda)
    : _callback_lambda(callback_lambda)
{
}

void Wrappers::SetRemoteSessionDescriptionObserver::OnSuccess()
{
    _callback_lambda(true, nullptr);
}

void Wrappers::SetRemoteSessionDescriptionObserver::OnFailure(RTCError error)
{
    _callback_lambda(false, error.message());
}