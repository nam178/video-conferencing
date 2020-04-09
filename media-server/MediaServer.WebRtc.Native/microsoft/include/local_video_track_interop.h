// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include "interop_api.h"

extern "C" {

//
// Wrapper
//

/// Add a reference to the native object associated with the given handle.
API_EXPORT void API_DEF_CONVT
mrsLocalVideoTrackAddRef(LocalVideoTrackHandle handle) noexcept;

/// Remove a reference from the native object associated with the given handle.
API_EXPORT void API_DEF_CONVT
mrsLocalVideoTrackRemoveRef(LocalVideoTrackHandle handle) noexcept;

/// Enable or disable a local video track. Enabled tracks output their media
/// content as usual. Disabled track output some void media content (black video
/// frames, silent audio frames). Enabling/disabling a track is a lightweight
/// concept similar to "mute", which does not require an SDP renegotiation.
API_EXPORT mrsResult API_DEF_CONVT
mrsLocalVideoTrackSetEnabled(LocalVideoTrackHandle track_handle,
                             mrsBool enabled) noexcept;

/// Query a local video track for its enabled status.
API_EXPORT mrsBool API_DEF_CONVT
mrsLocalVideoTrackIsEnabled(LocalVideoTrackHandle track_handle) noexcept;

}  // extern "C"
