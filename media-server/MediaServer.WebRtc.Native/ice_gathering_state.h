#pragma once

#include "pch.h"

extern "C"
{
    namespace Wrappers
    {
    enum class IceGatheringState : int32_t
    {
        New = 0,
        Gathering = 1,
        Complete = 2,
    };
    } // namespace Wrappers
}