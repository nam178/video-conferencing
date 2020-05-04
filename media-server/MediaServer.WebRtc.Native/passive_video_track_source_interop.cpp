#include "pch.h"

#include "passive_video_track_source.h"
#include "passive_video_track_source_interop.h"

using namespace webrtc;
using namespace Video;

EXPORT Video::PassiveVideoTrackSource *CONVENTION PassiveVideoTrackSourceCreate() noexcept
{
    return new PassiveVideoTrackSource();
}

EXPORT void CONVENTION
PassiveVideoTrackSourceRelease(Video::PassiveVideoTrackSource *video_track_source) noexcept
{
    video_track_source->Release();
}

EXPORT void CONVENTION
PassiveVideoTrackSourceAddRef(Video::PassiveVideoTrackSource *video_track_source) noexcept
{
    video_track_source->AddRef();
}