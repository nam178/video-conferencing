#pragma once

#include "pch.h"

#include "interop/global_factory.h"
#include "passive_video_track_source.h"
#include "refptr.h"
#include <atomic>

using namespace rtc;
using namespace webrtc;
using namespace Microsoft::MixedReality::WebRTC;

namespace PassiveVideo
{
class PassiveVideoTrack
{
  public:
    PassiveVideoTrack(const std::string &label,
                      PassiveVideoTrackSource *passive_video_track_source,
                      RefPtr<GlobalFactory> global_factory);

    // Get the native webrtc video track,
    // used to add this track into the peer connection
    scoped_refptr<VideoTrackInterface> VideoTrack();

  private:
    const std::string _label;
    RefPtr<GlobalFactory> _global_factory;
    scoped_refptr<VideoTrackInterface> _video_track;
    PassiveVideoTrackSource *_video_track_source;
};
} // namespace PassiveVideo
