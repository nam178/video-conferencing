#include "pch.h"

#include "create_session_description_observer.h"

MediaServer::CreateSessionDescriptionObserver::CreateSessionDescriptionObserver(
    std::function<void (Result<webrtc::SessionDescriptionInterface *>)> &&callback_lambda)
    : _callback_lambda(callback_lambda)
{
}

void MediaServer::CreateSessionDescriptionObserver::OnSuccess(
    webrtc::SessionDescriptionInterface *desc)
{
    // as per documentation, we own SessionDescriptionInterface,
    // and we'll delete it
    auto tmp2 = std::unique_ptr<webrtc::SessionDescriptionInterface>(desc);

    _callback_lambda(Result<webrtc::SessionDescriptionInterface *>{desc, true});
}

void MediaServer::CreateSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
{
    _callback_lambda(
        Result<webrtc::SessionDescriptionInterface *>{nullptr, false, error.message()});
}

MediaServer::CreateSessionDescriptionObserver::~CreateSessionDescriptionObserver()
{
    // confirm that this breakpoint gets hit
    ;
}