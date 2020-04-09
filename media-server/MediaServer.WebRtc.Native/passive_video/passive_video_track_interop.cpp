#include "pch.h"

#include "interop/global_factory.h"
#include "passive_video_track.h"
#include "passive_video_track_interop.h"
#include "passive_video_track_source.h"

using namespace Microsoft::MixedReality::WebRTC;
using namespace PassiveVideo;

API_EXPORT PassiveVideoTrackIntPtr API_DEF_CONVT
PassiveVideoTrackCreate(const char *video_track_name,
                        PassiveVideoTrackIntPtr passive_video_track_source)
{
    RefPtr<GlobalFactory> global_factory(GlobalFactory::InstancePtr());

    return new PassiveVideo::PassiveVideoTrack(
        video_track_name,
        static_cast<PassiveVideoTrackSource *>(passive_video_track_source),
        std::move(global_factory));
}

API_EXPORT void PassiveVideoTrackDestroy(PassiveVideoTrackIntPtr &video_track_ref) noexcept
{
    if(video_track_ref)
    {
        delete static_cast<PassiveVideo::PassiveVideoTrack *>(video_track_ref);
    }
    video_track_ref = nullptr;
}
