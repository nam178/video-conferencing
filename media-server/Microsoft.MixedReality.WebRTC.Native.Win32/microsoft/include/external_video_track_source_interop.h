// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include "interop_api.h"

extern "C" {

//
// Wrapper
//

/// Add a reference to the native object associated with the given handle.
API_EXPORT void API_DEF_CONVT mrsExternalVideoTrackSourceAddRef(
    ExternalVideoTrackSourceHandle handle) noexcept;

/// Remove a reference from the native object associated with the given handle.
API_EXPORT void API_DEF_CONVT mrsExternalVideoTrackSourceRemoveRef(
    ExternalVideoTrackSourceHandle handle) noexcept;

/// Create a custom video track source external to the implementation. This
/// allows feeding into WebRTC frames from any source, including generated or
/// synthetic frames, for example for testing. The frame is provided from a
/// callback as an I420-encoded buffer. This returns a handle to a newly
/// allocated object, which must be released once not used anymore with
/// |mrsExternalVideoTrackSourceRemoveRef()|.
API_EXPORT mrsResult API_DEF_CONVT mrsExternalVideoTrackSourceCreateFromI420ACallback(
    mrsRequestExternalI420AVideoFrameCallback callback,
    void* user_data,
    ExternalVideoTrackSourceHandle* source_handle_out) noexcept;

/// Create a custom video track source external to the implementation. This
/// allows feeding into WebRTC frames from any source, including generated or
/// synthetic frames, for example for testing. The frame is provided from a
/// callback as an ARGB32-encoded buffer. This returns a handle to a newly
/// allocated object, which must be released once not used anymore with
/// |mrsExternalVideoTrackSourceRemoveRef()|.
API_EXPORT mrsResult API_DEF_CONVT mrsExternalVideoTrackSourceCreateFromArgb32Callback(
    mrsRequestExternalArgb32VideoFrameCallback callback,
    void* user_data,
    ExternalVideoTrackSourceHandle* source_handle_out) noexcept;

/// Callback from the wrapper layer indicating that the wrapper has finished
/// creation, and it is safe to start sending frame requests to it. This needs
/// to be called after |mrsExternalVideoTrackSourceCreateFromI420ACallback()| or
/// |mrsExternalVideoTrackSourceCreateFromArgb32Callback()| to finish the
/// creation of the video track source and allow it to start capturing.
API_EXPORT void API_DEF_CONVT mrsExternalVideoTrackSourceFinishCreation(
    ExternalVideoTrackSourceHandle source_handle) noexcept;

/// Complete a video frame request with a provided I420A video frame.
API_EXPORT mrsResult API_DEF_CONVT mrsExternalVideoTrackSourceCompleteI420AFrameRequest(
    ExternalVideoTrackSourceHandle handle,
    uint32_t request_id,
    int64_t timestamp_ms,
    const mrsI420AVideoFrame* frame_view) noexcept;

/// Complete a video frame request with a provided ARGB32 video frame.
API_EXPORT mrsResult API_DEF_CONVT
mrsExternalVideoTrackSourceCompleteArgb32FrameRequest(
    ExternalVideoTrackSourceHandle handle,
    uint32_t request_id,
    int64_t timestamp_ms,
    const mrsArgb32VideoFrame* frame_view) noexcept;

/// Irreversibly stop the video source frame production and shutdown the video
/// source.
API_EXPORT void API_DEF_CONVT mrsExternalVideoTrackSourceShutdown(
    ExternalVideoTrackSourceHandle handle) noexcept;

}  // extern "C"
