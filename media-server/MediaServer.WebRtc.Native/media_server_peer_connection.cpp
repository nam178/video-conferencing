#include "pch.h"

#include "media_server_create_session_description_observer.h"
#include "media_server_peer_connection.h"

MediaServer::PeerConnection::PeerConnection(
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface)
    : _peer_connection_interface(std::move(peer_connection_interface))
{
}

void MediaServer::PeerConnection::CreateAnswer(
    Callback<Result<webrtc::SessionDescriptionInterface *>> callback)
{
    webrtc::PeerConnectionInterface::RTCOfferAnswerOptions opts{};

    _peer_connection_interface->CreateAnswer(
        new MediaServer::CreateSessionDescriptionObserver(std::move(callback)), opts);
}

webrtc::PeerConnectionInterface *MediaServer::PeerConnection::GetPeerConnectionInterface()
{
    return _peer_connection_interface.get();
}
