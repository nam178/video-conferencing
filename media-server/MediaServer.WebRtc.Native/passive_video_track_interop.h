#pragma once

#include "pch.h"

#include "export.h"

extern "C"
{
    using PassiveVideoTrackIntPtr = void *;
    using PassiveVideoTrackSourceIntrPtr = void*;

    // Destroy the PassiveVideoTrack and release the provided pointer
    EXPORT void CONVENTION
    PassiveVideoTrackDestroy(PassiveVideoTrackIntPtr &video_track_ref) noexcept;
}
