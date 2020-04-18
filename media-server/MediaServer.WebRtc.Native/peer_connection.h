#pragma once

#include "pch.h"

#include "create_answer_result.h"

using Success = bool;
using ErrorMessage = const char*;

namespace Wrappers
{
// manages the smart pointer to the libWebRTC peer connection
// so it won't get destroyed
class PeerConnection final
{
  public:
    // Contruct the PeerConnection resource wrapper
    // from a managed pointer.
    // This PeerConnection then takes ownership of said pointer.
    PeerConnection(rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface);

    // Create answer to an sdp offer.
    // the callback will be called on the signalling thread in theory.
    // The callback owns the SessionDescriptionInterface*
    void CreateAnswer(Callback<Wrappers::CreateAnswerResult> &&callback);

    // Completely kill this, implementation maps to native PeerConnection Close()
    void Close();

    // Set remote session description
    void RemoteSessionDescription(const char *sdp_type,
                                  const char *sdp,
                                  Callback<Success, ErrorMessage> callback);

    // get the raw pointer to the underlying native
    // PeerConnectionInterface
    webrtc::PeerConnectionInterface *GetPeerConnectionInterface();

  private:
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> _peer_connection_interface;
};
} // namespace Wrappers