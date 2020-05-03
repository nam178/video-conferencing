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
PassiveVideoTrackSourceDestroy(Video::PassiveVideoTrackSource *video_track_source) noexcept
{
    delete video_track_source;
}

EXPORT void CONVENTION
PassiveVideoTrackSourcePushVideoFrame(Video::PassiveVideoTrackSource *video_track_source,
                                      const VideoFrame &frame)
{
    static_cast<PassiveVideoTrackSource *>(video_track_source)->PushVideoFrame(frame);
}
