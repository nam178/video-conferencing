#include "pch.h"

#include "passive_video_track_source.h"
#include "passive_video_track_source_interop.h"

using namespace webrtc;
using namespace MediaSources;

EXPORT MediaSources::PassiveVideoTrackSource *CONVENTION PassiveVideoTrackSourceCreate() noexcept
{
    return new PassiveVideoTrackSource();
}

EXPORT void CONVENTION
PassiveVideoTrackSourceRelease(MediaSources::PassiveVideoTrackSource *video_track_source) noexcept
{
    video_track_source->Release();
}

EXPORT void CONVENTION
PassiveVideoTrackSourceAddRef(MediaSources::PassiveVideoTrackSource *video_track_source) noexcept
{
    video_track_source->AddRef();
}