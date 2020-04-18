#include "pch.h"

#include "peer_connection.h"
#include "peer_connection_interop.h"

void CONVENTION PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr)
{
    delete static_cast<Wrappers::PeerConnection *>(peer_connection_ptr);
}

void CONVENTION PeerConnectionClose(PeerConnectionPtr peer_connection_ptr)
{
    StaticCastOrThrow<Wrappers::PeerConnection>(peer_connection_ptr)->Close();
}

void CONVENTION PeerConnectionCreateAnswer(PeerConnectionPtr peer_connection_ptr,
                                           CreateAnswerCallback callback,
                                           UserData user_data)
{
    StaticCastOrThrow<Wrappers::PeerConnection>(peer_connection_ptr)
        ->CreateAnswer(Wrappers::Callback<Wrappers::CreateAnswerResult>{callback, user_data});
}

void CONVENTION
PeerConnectionSetRemoteSessionDescription(PeerConnectionPtr peer_connection_ptr,
                                          const char *sdp_type,
                                          const char *sdp,
                                          SetRemoteSessionDescriptionCallback callback,
                                          UserData user_data)
{
    StaticCastOrThrow<Wrappers::PeerConnection>(peer_connection_ptr)
        ->RemoteSessionDescription(
            sdp_type, sdp, Wrappers::Callback<Success, ErrorMessage>{callback, user_data});
}

bool CONVENTION PeerConnectionAddIceCandidate(PeerConnectionPtr peer_connection_ptr,
                                              const char *sdp_mid,
                                              uint32_t sdp_mline_index,
                                              const char *sdp,
                                              const char *error_message)
{
    std::string out_error{};
    auto result = StaticCastOrThrow<Wrappers::PeerConnection>(peer_connection_ptr)
                      ->AddIceCandiate(sdp_mid, sdp_mline_index, sdp, out_error);
    if(!result)
    {
        Utils::StringHelper::EnsureNullTerminatedCString(out_error);
        error_message = out_error.c_str();
    }
    return result;
}
