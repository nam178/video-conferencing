#pragma once

namespace Wrappers
{
struct CreateSdpResult
{
    // Whenever the sdp was created or not
    // If sdp was created, _sdp_type and _sdp should be set.
    bool _success = false;

    // If non success, the reason why
    const char *_error_message = nullptr;

    // answer, offer, pranswer or rollback, see
    // https://developer.mozilla.org/en-US/docs/Web/API/RTCSessionDescription
    const char *_sdp_type = nullptr;
    const char *_sdp = nullptr;
};
} // namespace Wrappers