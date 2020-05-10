#pragma once

#include "pch.h"

#include "media_stream_track.h"

namespace Shim
{
class RtpSender
{
  public:
    RtpSender(rtc::scoped_refptr<webrtc::RtpSenderInterface> native);

    webrtc::RtpSenderInterface *Native();

    // Not thread safe
    void SetTrack(Shim::MediaStreamTrack *track);

    // Not thread safe
    Shim::MediaStreamTrack *GetTrack();

  private:
    rtc::scoped_refptr<webrtc::RtpSenderInterface> _native;
    Shim::MediaStreamTrack *_track = nullptr;
};
} // namespace Shim