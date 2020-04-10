#pragma once

#include "pch.h"

#include "media_server_ice_candidate.h"
#include "media_server_ice_connection_state.h"
#include "media_server_ice_gathering_state.h"

namespace MediaServer
{
// Not thread safe,
// Designed so all methods must be called on signalling thread,
// or before this observer is added into PeerConnection
class PeerConnectionObserver final : public webrtc::PeerConnectionObserver
{
    using CandidateListPtr = void *;
    using TransceiverPtr = void *;

  public:
    // Override PeerConnectionObserver
    void OnRenegotiationNeeded() final override;
    void OnIceGatheringChange(
        webrtc::PeerConnectionInterface::IceGatheringState new_state) final override;
    void OnIceConnectionChange(
        webrtc::PeerConnectionInterface::IceConnectionState new_state) final override;
    void OnIceCandidate(const webrtc::IceCandidateInterface *ice_candidate) final override;
    void OnIceCandidatesRemoved(const std::vector<cricket::Candidate> &candidates) final override;
    void OnTrack(rtc::scoped_refptr<webrtc::RtpTransceiverInterface> transceiver) final override;

    // Methods for managed to register callbacks
    void SetRenegotiationNeededCallback(Callback<> &&callback) noexcept;
    void SetIceGatheringStateChangedCallback(Callback<IceGatheringState> &&callback) noexcept;
    void SetIceConnectionChangeCallback(Callback<IceConnectionState> &&callback) noexcept;
    void SetIceCandidateCallback(Callback<IceCandidate> &&callback) noexcept;
    void SetIceCandidatesRemovedCallback(Callback<CandidateListPtr> &&callback) noexcept;
    void SetTrackAddedCallback(Callback<TransceiverPtr> &&callback) noexcept;

  private:
    Callback<> _renegotiation_needed_callback{};
    Callback<IceGatheringState> _ice_gathering_state_changed_callback{};
    Callback<IceConnectionState> _ice_connection_change_callback{};
    Callback<IceCandidate> _ice_candidate_callback{};
    Callback<CandidateListPtr> _ice_candidates_removed_callback{};
    Callback<TransceiverPtr> _track_added_callback{};
};
} // namespace MediaServer