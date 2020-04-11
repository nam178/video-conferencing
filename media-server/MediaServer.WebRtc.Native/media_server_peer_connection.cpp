#include "pch.h"

#include "media_server_peer_connection.h"

MediaServer::PeerConnection::PeerConnection(
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface)
    : _peer_connection_interface(std::move(peer_connection_interface))
{

}

webrtc::PeerConnectionInterface *MediaServer::PeerConnection::GetPeerConnectionInterface()
{
    return _peer_connection_interface.get();
}
