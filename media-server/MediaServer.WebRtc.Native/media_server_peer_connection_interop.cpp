#include "pch.h"

#include "media_server_peer_connection.h"
#include "media_server_peer_connection_interop.h"

void CONVENTION PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr)
{
    delete static_cast<MediaServer::PeerConnection *>(peer_connection_ptr);
}
