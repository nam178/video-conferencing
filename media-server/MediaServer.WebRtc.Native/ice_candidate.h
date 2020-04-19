#pragma once

#include "pch.h"
extern "C"
{
    namespace Wrappers
    {
    struct IceCandidate
    {
        const char *_sdp = nullptr;
        const char *_sdp_mid = nullptr;
        int32_t _sdp_mline_index = 0;
    };
    } // namespace Wrappers
}
