#pragma once

#include "pch.h"

#include "create_answer_result.h"

extern "C"
{
    using PeerConnectionPtr = void *;
    using UserData = void *;
    using Success = bool;
    using ErrorMessage = const char *;
    using CreateAnswerCallback = void(CONVENTION *)(UserData, Wrappers::CreateAnswerResult);
    using SetRemoteSessionDescriptionCallback = void(CONVENTION *)(UserData, Success, ErrorMessage);

    EXPORT void CONVENTION PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionClose(PeerConnectionPtr peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionCreateAnswer(PeerConnectionPtr peer_connection_ptr,
                                                      CreateAnswerCallback callback,
                                                      UserData user_data);

    EXPORT void CONVENTION
    PeerConnectionSetRemoteSessionDescription(PeerConnectionPtr peer_connection_ptr,
                                              const char *sdp_type,
                                              const char *sdp,
                                              SetRemoteSessionDescriptionCallback callback,
                                              UserData user_data);
}