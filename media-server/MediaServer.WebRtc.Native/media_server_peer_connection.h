#pragma once

#include "pch.h"

namespace MediaServer
{
// manages the smart pointer to the libWebRTC peer connection
// so it won't get destroyed
class PeerConnection final
{
  public:
    PeerConnection(rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface);
    webrtc::PeerConnectionInterface *GetPeerConnectionInterface();
  private:
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> _peer_connection_interface;
};
} // namespace MediaServer