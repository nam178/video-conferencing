#pragma once

#include "pch.h"

#include "create_answer_result.h"

extern "C"
{
    using PeerConnectionPtr = void *;
    using UserData = void *;
    using Callback = void(CONVENTION *)(UserData, MediaServer::CreateAnswerResult);

    EXPORT void CONVENTION PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionClose(PeerConnectionPtr peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionCreateAnswer(PeerConnectionPtr peer_connection_ptr,
                                                      Callback callback,
                                                      UserData user_data);

    EXPORT void CONVENTION
    PeerConnectionSetRemoteSessionDescription(PeerConnectionPtr peer_connection_ptr,
                                              const char *sdp_type,
                                              const char *sdp);
}