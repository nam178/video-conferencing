#pragma once

#include "pch.h"

namespace Wrappers
{
enum class IceConnectionState : int32_t
{
    IceConnectionNew = 0,
    IceConnectionChecking = 1,
    IceConnectionConnected = 2,
    IceConnectionCompleted = 3,
    IceConnectionFailed = 4,
    IceConnectionDisconnected = 5,
    IceConnectionClosed = 6,
    IceConnectionMax = 7
};
} // namespace Wrappers