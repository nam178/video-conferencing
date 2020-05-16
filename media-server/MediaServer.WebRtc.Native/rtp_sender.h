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

    void SetStreamId(const char *stream_id);

    const char *GetStreamId();

    // Not thread safe
    Shim::MediaStreamTrack *GetTrack();

  private:
    rtc::scoped_refptr<webrtc::RtpSenderInterface> _native;
    Shim::MediaStreamTrack *_track = nullptr;
    std::string _stream_id{};
    std::mutex _stream_id_mutex{};
};
} // namespace Shim