#pragma once

namespace MediaServer
{
struct IceCandidate
{
    IceCandidate(const char *sdp, const char *sdp_mid, int sdp_mline_index)
        : _sdp(sdp), _sdp_mid(_sdp_mid), _sdp_mline_index(sdp_mline_index)
    {
    }
    const char *_sdp = nullptr;
    const char *_sdp_mid = nullptr;
    int _sdp_mline_index = 0;
};
} // namespace MediaServer
