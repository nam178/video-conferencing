#include "pch.h"

#include "passive_video_track_source.h"
#include "passive_video_track_source_interop.h"

using namespace webrtc;
using namespace PassiveVideo;

API_EXPORT PassiveVideoTrackSourceIntPtr API_DEF_CONVT PassiveVideoTrackSourceCreate() noexcept
{
    return new PassiveVideoTrackSource();
}

API_EXPORT void API_DEF_CONVT
PassiveVideoTrackSourceDestroy(PassiveVideoTrackSourceIntPtr &video_track_source_ref) noexcept
{
    delete video_track_source_ref;
    video_track_source_ref = nullptr;
}

API_EXPORT void API_DEF_CONVT
PassiveVideoTrackSourcePushVideoFrame(PassiveVideoTrackSourceIntPtr video_track_source,
                                      const VideoFrame &frame)
{
    static_cast<PassiveVideoTrackSource *>(video_track_source)->PushVideoFrame(frame);
}
