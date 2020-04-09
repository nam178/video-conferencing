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
mrsPeerConnectionAddRef(PeerConnectionHandle handle) noexcept;

/// Remove a reference from the native object associated with the given handle.
API_EXPORT void API_DEF_CONVT
mrsPeerConnectionRemoveRef(PeerConnectionHandle handle) noexcept;

/// Callback fired when the state of the ICE connection changed.
using mrsPeerConnectionIceGatheringStateChangedCallback =
    void(API_DEF_CONVT*)(void* user_data, IceGatheringState new_state);

/// Register a callback fired when the ICE connection state changes.
API_EXPORT void API_DEF_CONVT mrsPeerConnectionRegisterIceGatheringStateChangedCallback(
    PeerConnectionHandle peer_handle,
    mrsPeerConnectionIceGatheringStateChangedCallback callback,
    void* user_data) noexcept;

}  // extern "C"
