// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// This is a precompiled header, it must be on its own, followed by a blank
// line, to prevent clang-format from reordering it with other headers.
#include "pch.h"

#include "local_video_track_interop.h"
#include "media/local_video_track.h"

using namespace Microsoft::MixedReality::WebRTC;

void CONVENTION mrsLocalVideoTrackAddRef(LocalVideoTrackHandle handle) noexcept {
  if (auto track = static_cast<LocalVideoTrack*>(handle)) {
    track->AddRef();
  } else {
    RTC_LOG(LS_WARNING)
        << "Trying to add reference to NULL LocalVideoTrack object.";
  }
}

void CONVENTION
mrsLocalVideoTrackRemoveRef(LocalVideoTrackHandle handle) noexcept {
  if (auto track = static_cast<LocalVideoTrack*>(handle)) {
    track->RemoveRef();
  } else {
    RTC_LOG(LS_WARNING) << "Trying to remove reference from NULL "
                           "LocalVideoTrack object.";
  }
}

mrsResult CONVENTION
mrsLocalVideoTrackSetEnabled(LocalVideoTrackHandle track_handle,
                             mrsBool enabled) noexcept {
  auto track = static_cast<LocalVideoTrack*>(track_handle);
  if (!track) {
    return Result::kInvalidParameter;
  }
  track->SetEnabled(enabled != mrsBool::kFalse);
  return Result::kSuccess;
}

mrsBool CONVENTION
mrsLocalVideoTrackIsEnabled(LocalVideoTrackHandle track_handle) noexcept {
  auto track = static_cast<LocalVideoTrack*>(track_handle);
  if (!track) {
    return mrsBool::kFalse;
  }
  return (track->IsEnabled() ? mrsBool::kTrue : mrsBool::kFalse);
}
