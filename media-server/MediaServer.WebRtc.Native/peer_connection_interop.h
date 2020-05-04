#pragma once

#include "pch.h"

#include "create_sdp_result.h"
#include "media_stream_track.h"

extern "C"
{
    using UserData = void *;
    using Success = bool;
    using ErrorMessage = const char *;
    using CreateSdpCallback = void(CONVENTION *)(UserData, Wrappers::CreateSdpResult);
    using SetRemoteSessionDescriptionCallback = void(CONVENTION *)(UserData, Success, ErrorMessage);

    EXPORT void CONVENTION PeerConnectionDestroy(Wrappers::PeerConnection *peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionClose(Wrappers::PeerConnection *peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionCreateAnswer(Wrappers::PeerConnection *peer_connection_ptr,
                                                      CreateSdpCallback callback,
                                                      UserData user_data);

    EXPORT void CONVENTION PeerConnectionCreateOffer(Wrappers::PeerConnection *peer_connection_ptr,
                                                     CreateSdpCallback callback,
                                                     UserData user_data);

    EXPORT void CONVENTION
    PeerConnectionSetRemoteSessionDescription(Wrappers::PeerConnection *peer_connection_ptr,
                                              const char *sdp_type,
                                              const char *sdp,
                                              SetRemoteSessionDescriptionCallback callback,
                                              UserData user_data);

    EXPORT void CONVENTION
    PeerConnectionSetLocalSessionDescription(Wrappers::PeerConnection *peer_connection_ptr,
                                             const char *sdp_type,
                                             const char *sdp,
                                             SetRemoteSessionDescriptionCallback callback,
                                             UserData user_data);

    EXPORT bool CONVENTION
    PeerConnectionAddIceCandidate(Wrappers::PeerConnection *peer_connection_ptr,
                                  const char *sdp_mid,
                                  int32_t sdp_mline_index,
                                  const char *sdp);

    EXPORT Wrappers::RtpSender *CONVENTION
    PeerConnectionAddTrack(Wrappers::PeerConnection *peer_connection,
                           Wrappers::MediaStreamTrack *media_stream_track,
                           const char *stream_id);

    EXPORT void CONVENTION PeerConnectionRemoveTrack(Wrappers::PeerConnection *peer_connection,
                                                     Wrappers::RtpSender *rtp_sender);
}