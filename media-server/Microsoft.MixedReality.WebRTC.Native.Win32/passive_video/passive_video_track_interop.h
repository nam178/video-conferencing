#pragma once

#include "pch.h"

#include "export.h"

extern "C"
{
    using PassiveVideoTrackIntPtr = void *;
    using PassiveVideoTrackSourceIntrPtr = void*;

    // Create new PassiveVideoTrack and return its pointer
    API_EXPORT PassiveVideoTrackIntPtr API_DEF_CONVT
    PassiveVideoTrackCreate(const char *video_track_name, PassiveVideoTrackIntPtr passive_video_track_source);

    // Destroy the PassiveVideoTrack and release the provided pointer
    API_EXPORT void API_DEF_CONVT
    PassiveVideoTrackDestroy(PassiveVideoTrackIntPtr &video_track_ref) noexcept;
}
