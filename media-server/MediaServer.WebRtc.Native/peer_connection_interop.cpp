#include "pch.h"

#include "peer_connection.h"
#include "peer_connection_interop.h"

void CONVENTION PeerConnectionDestroy(Shim::PeerConnection *peer_connection_ptr)
{
    delete peer_connection_ptr;
}

void CONVENTION PeerConnectionClose(Shim::PeerConnection *peer_connection_ptr)
{
    peer_connection_ptr->Close();
}

void CONVENTION PeerConnectionCreateAnswer(Shim::PeerConnection *peer_connection_ptr,
                                           CreateSdpCallback callback,
                                           UserData user_data)
{
    peer_connection_ptr->CreateAnswer(Shim::Callback<Shim::CreateSdpResult>{callback, user_data});
}

void CONVENTION PeerConnectionCreateOffer(Shim::PeerConnection *peer_connection_ptr,
                                          CreateSdpCallback callback,
                                          UserData user_data)
{
    peer_connection_ptr->CreateOffer(Shim::Callback<Shim::CreateSdpResult>{callback, user_data});
}

void CONVENTION
PeerConnectionSetRemoteSessionDescription(Shim::PeerConnection *peer_connection_ptr,
                                          const char *sdp_type,
                                          const char *sdp,
                                          SetRemoteSessionDescriptionCallback callback,
                                          UserData user_data)
{
    peer_connection_ptr->RemoteSessionDescription(
        sdp_type, sdp, Shim::Callback<Success, ErrorMessage>{callback, user_data});
}

void CONVENTION
PeerConnectionSetLocalSessionDescription(Shim::PeerConnection *peer_connection_ptr,
                                         const char *sdp_type,
                                         const char *sdp,
                                         SetRemoteSessionDescriptionCallback callback,
                                         UserData user_data)
{
    peer_connection_ptr->LocalSessionDescription(
        sdp_type, sdp, Shim::Callback<Success, ErrorMessage>{callback, user_data});
}

bool CONVENTION PeerConnectionAddIceCandidate(Shim::PeerConnection *peer_connection_ptr,
                                              const char *sdp_mid,
                                              int32_t sdp_mline_index,
                                              const char *sdp)
{
    std::string out_error{};
    auto success = peer_connection_ptr->AddIceCandiate(sdp_mid, sdp_mline_index, sdp, out_error);
    if(!success)
    {
        Utils::StringHelper::EnsureNullTerminatedCString(out_error);
        RTC_LOG(LS_ERROR) << "Failed adding candidate: " << out_error;
    }
    return success;
}

Shim::RtpSender *CONVENTION PeerConnectionAddTrack(Shim::PeerConnection *peer_connection,
                                                   Shim::MediaStreamTrack *media_stream_track,
                                                   const char *stream_id)
{
    std::string stream_id_str(stream_id);
    std::vector<std::string> stream_ids{stream_id_str};
    auto result = peer_connection->AddTrack(media_stream_track->GetMediaStreamTrack(), stream_ids);
    if(result)
    {
        return result.release();
    }
    return nullptr;
}

void CONVENTION PeerConnectionRemoveTrack(Shim::PeerConnection *peer_connection,
                                          Shim::RtpSender *rtp_sender)
{
    if(!peer_connection)
    {
        RTC_LOG(LS_ERROR) << "peer_connection is NULL";
        throw new std::runtime_error("peer_connection is NULL");
    }
    peer_connection->RemoveTrack(rtp_sender);
}

void CONVENTION PeerConnectionGetTransceivers(Shim::PeerConnection *peer_connection,
                                              Shim::RtpTransceiver ***transceivers,
                                              int32_t *size)
{
    peer_connection->GetTransceivers(transceivers, size);
}

void CONVENTION PeerConnectionFreeGetTransceiversResult(Shim::PeerConnection *peer_connection,
                                                        Shim::RtpTransceiver **transceivers)
{
    peer_connection->FreeGetTransceiversResult(transceivers);
}
