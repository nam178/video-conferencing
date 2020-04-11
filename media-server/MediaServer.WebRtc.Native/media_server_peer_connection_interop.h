#pragma once

#include "pch.h"

extern "C"
{
    using PeerConnectionPtr = void *;

    EXPORT void CONVENTION
    PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr);
}