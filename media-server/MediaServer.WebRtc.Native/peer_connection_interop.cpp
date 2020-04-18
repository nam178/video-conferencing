#include "pch.h"

#include "peer_connection.h"
#include "peer_connection_interop.h"

void CONVENTION PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr)
{
    delete static_cast<MediaServer::PeerConnection *>(peer_connection_ptr);
}

void CONVENTION PeerConnectionClose(PeerConnectionPtr peer_connection_ptr)
{
    auto tmp = static_cast<MediaServer::PeerConnection *>(peer_connection_ptr);
    if(!tmp)
    {
        throw new std::runtime_error("peer_connection_ptr is NULL");
    }
    tmp->Close();
}

void CONVENTION PeerConnectionCreateAnswer(PeerConnectionPtr peer_connection_ptr,
                                           Callback callback,
                                           UserData user_data)
{
    auto peer_connection = static_cast<MediaServer::PeerConnection *>(peer_connection_ptr);
    if(!peer_connection)
    {
        RTC_LOG(LS_ERROR, "peer_connection_ptr null pointer");
        throw new std::runtime_error("peer_connection_ptr null pointer");
    }

    peer_connection->CreateAnswer(
        MediaServer::Callback<MediaServer::CreateAnswerResult>{callback, user_data});
}

void CONVENTION PeerConnectionSetRemoteSessionDescription(PeerConnectionPtr peer_connection_ptr,
                                                          const char *sdp_type,
                                                          const char *sdp)
{
    auto peer_connection = static_cast<MediaServer::PeerConnection *>(peer_connection_ptr);
    if(!peer_connection)
    {
        RTC_LOG(LS_ERROR, "peer_connection is NULL");
        throw new std::runtime_error("peer_connection is NULL");
    }
    peer_connection->RemoteSessionDescription(sdp_type, sdp);
}
