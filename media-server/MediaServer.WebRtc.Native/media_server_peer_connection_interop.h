#pragma once

#include "pch.h"

#include "media_server_create_answer_result.h"

extern "C"
{
    using PeerConnectionPtr = void *;
    using UserData = void *;
    using Callback = void(CONVENTION *)(UserData, MediaServer::CreateAnswerResult);

    EXPORT void CONVENTION PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr);

    EXPORT void CONVENTION PeerConnectionCreateAnswer(PeerConnectionPtr peer_connection_ptr,
                                                      Callback callback,
                                                      UserData user_data);
}