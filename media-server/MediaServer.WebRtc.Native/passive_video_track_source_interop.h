#pragma once

#include "pch.h"

#include "export.h"

using namespace webrtc;

extern "C"
{
    using PassiveVideoTrackSourceIntPtr = void *;

    EXPORT PassiveVideoTrackSourceIntPtr CONVENTION PassiveVideoTrackSourceCreate() noexcept;

    EXPORT void CONVENTION
    PassiveVideoTrackSourceDestroy(PassiveVideoTrackSourceIntPtr &video_track_source_ref) noexcept;

    EXPORT void CONVENTION
    PassiveVideoTrackSourcePushVideoFrame(PassiveVideoTrackSourceIntPtr video_track_source,
                                          const VideoFrame &frame);
}
