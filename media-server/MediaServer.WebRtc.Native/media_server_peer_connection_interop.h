#pragma once

#include "pch.h"

extern "C"
{
    using PeerConnectionPtr = void *;

    EXPORT PeerConnectionPtr CONVENTION
    PeerConnectionDestroy(PeerConnectionPtr peer_connection_ptr);
}