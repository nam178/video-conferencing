#pragma once

#include "pch.h"

#include "export.h"
#include "passive_video_track_source.h"

using namespace webrtc;

extern "C"
{
    EXPORT Video::PassiveVideoTrackSource *CONVENTION PassiveVideoTrackSourceCreate() noexcept;

    EXPORT void CONVENTION
    PassiveVideoTrackSourceRelease(Video::PassiveVideoTrackSource *video_track_source) noexcept;
    
    EXPORT void CONVENTION
    PassiveVideoTrackSourceAddRef(Video::PassiveVideoTrackSource *video_track_source) noexcept;
}
