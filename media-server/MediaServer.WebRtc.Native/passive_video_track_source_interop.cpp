#include "pch.h"

#include "passive_video_track_source.h"
#include "passive_video_track_source_interop.h"

using namespace webrtc;
using namespace Video;

EXPORT PassiveVideoTrackSourceIntPtr CONVENTION PassiveVideoTrackSourceCreate() noexcept
{
    return new PassiveVideoTrackSource();
}

EXPORT void CONVENTION
PassiveVideoTrackSourceDestroy(PassiveVideoTrackSourceIntPtr &video_track_source_ref) noexcept
{
    delete video_track_source_ref;
    video_track_source_ref = nullptr;
}

EXPORT void CONVENTION
PassiveVideoTrackSourcePushVideoFrame(PassiveVideoTrackSourceIntPtr video_track_source,
                                      const VideoFrame &frame)
{
    static_cast<PassiveVideoTrackSource *>(video_track_source)->PushVideoFrame(frame);
}
