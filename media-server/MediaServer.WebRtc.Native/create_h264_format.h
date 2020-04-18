#pragma once

#include "pch.h"

#include "media/base/h264_profile_level_id.h"
#include "modules/video_coding/codecs/h264/include/h264.h"

using namespace webrtc;

namespace NoopVideo
{
SdpVideoFormat CreateH264Format(H264::Profile profile, H264::Level level,
                                const std::string &packetization_mode);
} // namespace NoopVideo
