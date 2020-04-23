#pragma once

#include "pch.h"

#include "media_stream_track.h"

namespace Wrappers
{
class RtpReceiver
{
  public:
    RtpReceiver(rtc::scoped_refptr<webrtc::RtpReceiverInterface> &&rtp_receiver);

    // Get the RtpReceiverInterface associated with this wrapper.
    // Note this wrapper still owns the returned pointer.
    webrtc::RtpReceiverInterface *GetRtpReceiverInterface() const;

    // Get the wrapper for MediaStreamTrack from webrtc::RtpReceiverInterface above.
    // This wrapper does not own the return track.
    std::unique_ptr<MediaStreamTrack> GetTrack();

  private:
    rtc::scoped_refptr<webrtc::RtpReceiverInterface> _rtp_receiver;
};
} // namespace Wrappers
