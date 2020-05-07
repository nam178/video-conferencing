#include "pch.h"

#include "create_session_description_observer.h"

Shim::CreateSessionDescriptionObserver::CreateSessionDescriptionObserver(
    std::function<void(Result<webrtc::SessionDescriptionInterface *>)> &&callback_lambda)
    : _callback_lambda(callback_lambda)
{
}

void Shim::CreateSessionDescriptionObserver::OnSuccess(
    webrtc::SessionDescriptionInterface *desc)
{
    // as per documentation, we own SessionDescriptionInterface,
    // therefore we will delete it.
    auto tmp = std::unique_ptr<webrtc::SessionDescriptionInterface>(desc);

    _callback_lambda(Result<webrtc::SessionDescriptionInterface *>{desc, true});
}

void Shim::CreateSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
{
    _callback_lambda(
        Result<webrtc::SessionDescriptionInterface *>{nullptr, false, error.message()});
}