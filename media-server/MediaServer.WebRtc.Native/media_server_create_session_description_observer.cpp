#include "pch.h"

#include "media_server_create_session_description_observer.h"

MediaServer::CreateSessionDescriptionObserver::CreateSessionDescriptionObserver(
    Callback<Result<webrtc::SessionDescriptionInterface *>> &&callback)
    : _callback(callback)
{
}

void MediaServer::CreateSessionDescriptionObserver::OnSuccess(
    webrtc::SessionDescriptionInterface *desc)
{
    auto tmp = std::unique_ptr<CreateSessionDescriptionObserver>(this); // self delete
    _callback(Result<webrtc::SessionDescriptionInterface *>{desc, true});
}

void MediaServer::CreateSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
{
    auto tmp = std::unique_ptr<CreateSessionDescriptionObserver>(this); // self delete
    _callback(Result<webrtc::SessionDescriptionInterface *>{nullptr, false, error.message()});
}
