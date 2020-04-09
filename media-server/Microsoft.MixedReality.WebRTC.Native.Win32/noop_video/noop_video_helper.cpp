#include "pch.h"

#include "noop_video_helper.h"

using namespace webrtc;

namespace NoopVideo
{
SdpVideoFormat CreateH264Format(H264::Profile profile, H264::Level level,
                                const std::string &packetization_mode)
{
    const absl::optional<std::string> profile_string =
        H264::ProfileLevelIdToString(H264::ProfileLevelId(profile, level));
    RTC_CHECK(profile_string);
    return SdpVideoFormat(cricket::kH264CodecName,
                          {{cricket::kH264FmtpProfileLevelId, *profile_string},
                           {cricket::kH264FmtpLevelAsymmetryAllowed, "1"},
                           {cricket::kH264FmtpPacketizationMode, packetization_mode}});
}
} // namespace NoopVideo
