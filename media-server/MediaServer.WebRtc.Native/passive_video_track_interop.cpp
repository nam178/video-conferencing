#include "pch.h"

#include "passive_video_track.h"
#include "passive_video_track_interop.h"
#include "passive_video_track_source.h"

using namespace PassiveVideo;

EXPORT void PassiveVideoTrackDestroy(PassiveVideoTrackIntPtr &video_track_ref) noexcept
{
    if(video_track_ref)
    {
        delete static_cast<PassiveVideo::PassiveVideoTrack *>(video_track_ref);
    }
    video_track_ref = nullptr;
}
