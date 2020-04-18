#pragma once

#include "pch.h"

namespace Wrappers
{
struct IceCandidate
{
    const char *_sdp = nullptr;
    const char *_sdp_mid = nullptr;
    int _sdp_mline_index = 0;
};
} // namespace Wrappers
