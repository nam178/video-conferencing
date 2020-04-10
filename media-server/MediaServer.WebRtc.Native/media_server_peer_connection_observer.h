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
    using CandidateListPtr = const void *;
    using RtpTransceiverInterfacePtr = void *;
    using RtpReceiverInterfacePtr = void *;

  public:
    PeerConnectionObserver();

    // Override PeerConnectionObserver
    void OnRenegotiationNeeded() override;
    void OnIceGatheringChange(
        webrtc::PeerConnectionInterface::IceGatheringState new_state) override;
    void OnIceConnectionChange(
        webrtc::PeerConnectionInterface::IceConnectionState new_state) override;
    void OnIceCandidate(const webrtc::IceCandidateInterface *ice_candidate) override;
    void OnIceCandidatesRemoved(const std::vector<cricket::Candidate> &candidates) override;
    void OnTrack(rtc::scoped_refptr<webrtc::RtpTransceiverInterface> transceiver) override;
    void OnRemoveTrack(rtc::scoped_refptr<webrtc::RtpReceiverInterface> receiver) override;
    void OnSignalingChange(webrtc::PeerConnectionInterface::SignalingState new_state) override;
    void OnDataChannel(rtc::scoped_refptr<webrtc::DataChannelInterface> data_channel) override;

    // Methods for managed to register callbacks
    void SetRenegotiationNeededCallback(Callback<> &&callback) noexcept;
    void SetIceGatheringStateChangedCallback(Callback<IceGatheringState> &&callback) noexcept;
    void SetIceConnectionChangeCallback(Callback<IceConnectionState> &&callback) noexcept;
    // if the name is confusing,
    // this occurs when an ice candidate is added.
    void SetIceCandidateCallback(Callback<IceCandidate> &&callback) noexcept;
    void SetIceCandidatesRemovedCallback(Callback<CandidateListPtr> &&callback) noexcept;
    void SetRemoteTrackAddedCallback(Callback<RtpTransceiverInterfacePtr> &&callback) noexcept;
    void SetRemoteTrackRemovedCallback(Callback<RtpReceiverInterfacePtr> &&callback) noexcept;

  private:
    Callback<> _renegotiation_needed_callback{};
    Callback<IceGatheringState> _ice_gathering_state_changed_callback{};
    Callback<IceConnectionState> _ice_connection_change_callback{};
    Callback<IceCandidate> _ice_candidate_callback{};
    Callback<CandidateListPtr> _ice_candidates_removed_callback{};
    Callback<RtpTransceiverInterfacePtr> _remote_track_added_callback{};
    Callback<RtpReceiverInterfacePtr> _remote_track_removed_callback{};
};
} // namespace MediaServer