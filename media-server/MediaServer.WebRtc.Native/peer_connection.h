#pragma once

#include "pch.h"

#include "create_sdp_result.h"
#include "rtp_sender.h"
#include "rtp_transceiver.h"

using Success = bool;
using ErrorMessage = const char *;

namespace Shim
{
// manages the smart pointer to the libWebRTC peer connection
// so it won't get destroyed
class PeerConnection final
{
  public:
    // Contruct the PeerConnection resource wrapper
    // from transceiver_entry managed pointer.
    // This PeerConnection then takes ownership of said pointer.
    PeerConnection(rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface);

    // Create an offer, will be executed in signalling thread by the lib
    void CreateOffer(Callback<Shim::CreateSdpResult> &&callback);

    // Create answer to an sdp offer.
    // the callback will be called on the signalling thread in theory.
    // The callback owns the SessionDescriptionInterface*
    void CreateAnswer(Callback<Shim::CreateSdpResult> &&callback);

    // Completely kill this, implementation maps to native PeerConnection Close()
    void Close();

    // Add ice candiadate,
    // should only call ths after RemoveSessionDescription() is set
    bool AddIceCandiate(const char *sdp_mid,
                        int sdp_mline_index,
                        const char *sdp,
                        std::string &out_error);

    // Set remote session description
    void RemoteSessionDescription(const char *sdp_type,
                                  const char *sdp,
                                  Callback<Success, ErrorMessage> callback);

    // Set local session description
    // Must be called immediately after CreateAnswer()
    void LocalSessionDescription(const char *sdp_type,
                                 const char *sdp,
                                 Callback<Success, ErrorMessage> callback);

    // Add and remove local tracks,
    // and return the rtp sender associated with this track
    std::unique_ptr<Shim::RtpSender> AddTrack(
        rtc::scoped_refptr<webrtc::MediaStreamTrackInterface> track,
        const std::vector<std::string> &stream_ids);

    void RemoveTrack(Shim::RtpSender *rtp_sender);

    // get the raw pointer to the underlying native
    // PeerConnectionInterface
    webrtc::PeerConnectionInterface *GetPeerConnectionInterface();

    // Get and rebuild known transceivers.
    // Caller owns the RtpTransceiver, however it must manually call FreeGetTransceiversResult()
    // to release the array used for the output.
    // Not thread safe.
    void GetTransceivers(Shim::RtpTransceiver ***transceiver, int32_t *size);

    // Free the memory allocated for the GetTransceivers() call above
    void FreeGetTransceiversResult(Shim::RtpTransceiver **transceiver) const;
  private:
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> _peer_connection_interface;
    std::unordered_map<webrtc::RtpTransceiverInterface *, Shim::RtpTransceiver *>
        _last_known_transceivers{};
};
} // namespace Shim