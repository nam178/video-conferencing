#pragma once

#include "pch.h"

#include "create_sdp_result.h"
#include "media_stream_track.h"

extern "C"
{
    using UserData = void *;
    using Success = bool;
    using ErrorMessage = const char *;
    using CreateSdpCallback = void(CONVENTION *)(UserData, Shim::CreateSdpResult);
    using SetRemoteSessionDescriptionCallback = void(CONVENTION *)(UserData, Success, ErrorMessage);

    EXPORT void CONVENTION PeerConnectionDestroy(Shim::PeerConnection *peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionClose(Shim::PeerConnection *peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionCreateAnswer(Shim::PeerConnection *peer_connection_ptr,
                                                      CreateSdpCallback callback,
                                                      UserData user_data);

    EXPORT void CONVENTION PeerConnectionCreateOffer(Shim::PeerConnection *peer_connection_ptr,
                                                     CreateSdpCallback callback,
                                                     UserData user_data);

    EXPORT void CONVENTION
    PeerConnectionSetRemoteSessionDescription(Shim::PeerConnection *peer_connection_ptr,
                                              const char *sdp_type,
                                              const char *sdp,
                                              SetRemoteSessionDescriptionCallback callback,
                                              UserData user_data);

    EXPORT void CONVENTION
    PeerConnectionSetLocalSessionDescription(Shim::PeerConnection *peer_connection_ptr,
                                             const char *sdp_type,
                                             const char *sdp,
                                             SetRemoteSessionDescriptionCallback callback,
                                             UserData user_data);

    EXPORT bool CONVENTION PeerConnectionAddIceCandidate(Shim::PeerConnection *peer_connection_ptr,
                                                         const char *sdp_mid,
                                                         int32_t sdp_mline_index,
                                                         const char *sdp);

    EXPORT Shim::RtpSender *CONVENTION
    PeerConnectionAddTrack(Shim::PeerConnection *peer_connection,
                           Shim::MediaStreamTrack *media_stream_track,
                           const char *stream_id);

    EXPORT void CONVENTION PeerConnectionRemoveTrack(Shim::PeerConnection *peer_connection,
                                                     Shim::RtpSender *rtp_sender);

    EXPORT void CONVENTION PeerConnectionGetTransceivers(Shim::PeerConnection *peer_connection,
                                                         Shim::RtpTransceiver ***transceivers,
                                                         int32_t *size);

    EXPORT void CONVENTION
    PeerConnectionFreeGetTransceiversResult(Shim::PeerConnection *peer_connection,
                                            Shim::RtpTransceiver **transceivers);

    EXPORT Shim::RtpTransceiver *CONVENTION
    PeerConnectionAddTransceiver(Shim::PeerConnection *peer_connection,
                                 bool is_audio_transceiver,
                                 Shim::RtpTransceiverDirection direction);
}