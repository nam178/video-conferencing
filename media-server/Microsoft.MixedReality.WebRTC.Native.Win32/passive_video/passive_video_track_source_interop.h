#pragma once

#include "pch.h"

#include "export.h"

using namespace webrtc;

extern "C"
{
    using PassiveVideoTrackSourceIntPtr = void *;

    API_EXPORT PassiveVideoTrackSourceIntPtr API_DEF_CONVT PassiveVideoTrackSourceCreate() noexcept;

    API_EXPORT void API_DEF_CONVT
    PassiveVideoTrackSourceDestroy(PassiveVideoTrackSourceIntPtr &video_track_source_ref) noexcept;

    API_EXPORT void API_DEF_CONVT
    PassiveVideoTrackSourcePushVideoFrame(PassiveVideoTrackSourceIntPtr video_track_source,
                                          const VideoFrame &frame);
}
