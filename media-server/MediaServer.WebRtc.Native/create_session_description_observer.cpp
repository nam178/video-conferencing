#include "pch.h"

#include "create_session_description_observer.h"

Wrappers::CreateSessionDescriptionObserver::CreateSessionDescriptionObserver(
    std::function<void(Result<webrtc::SessionDescriptionInterface *>)> &&callback_lambda)
    : _callback_lambda(callback_lambda)
{
}

void Wrappers::CreateSessionDescriptionObserver::OnSuccess(
    webrtc::SessionDescriptionInterface *desc)
{
    // as per documentation, we own SessionDescriptionInterface,
    // therefore we will delete it.
    auto tmp = std::unique_ptr<webrtc::SessionDescriptionInterface>(desc);

    _callback_lambda(Result<webrtc::SessionDescriptionInterface *>{desc, true});
}

void Wrappers::CreateSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
{
    _callback_lambda(
        Result<webrtc::SessionDescriptionInterface *>{nullptr, false, error.message()});
}

void Wrappers::CreateSessionDescriptionObserver::OnFailure(const std::string &error)
{
    _callback_lambda(Result<webrtc::SessionDescriptionInterface *>{nullptr, false, error.c_str()});
}

Wrappers::CreateSessionDescriptionObserver::~CreateSessionDescriptionObserver()
{
    // confirm that this breakpoint gets hit
    ;
}