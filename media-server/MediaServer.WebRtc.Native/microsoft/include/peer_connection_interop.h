// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include "interop_api.h"

extern "C" {

//
// Wrapper
//

/// Add a reference to the native object associated with the given handle.
EXPORT void CONVENTION
mrsPeerConnectionAddRef(PeerConnectionHandle handle) noexcept;

/// Remove a reference from the native object associated with the given handle.
EXPORT void CONVENTION
mrsPeerConnectionRemoveRef(PeerConnectionHandle handle) noexcept;

/// Callback fired when the state of the ICE connection changed.
using mrsPeerConnectionIceGatheringStateChangedCallback =
    void(CONVENTION*)(void* user_data, IceGatheringState new_state);

/// Register a callback fired when the ICE connection state changes.
EXPORT void CONVENTION mrsPeerConnectionRegisterIceGatheringStateChangedCallback(
    PeerConnectionHandle peer_handle,
    mrsPeerConnectionIceGatheringStateChangedCallback callback,
    void* user_data) noexcept;

}  // extern "C"
