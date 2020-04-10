#include "pch.h"

#include "media_server_peer_connection_observer.h"
#include "utils_string_helper.h"

void MediaServer::PeerConnectionObserver::SetIceGatheringStateChangedCallback(
    Callback<IceGatheringState> &&callback) noexcept
{
    _ice_gathering_state_changed_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::SetIceConnectionChangeCallback(
    Callback<IceConnectionState> &&callback) noexcept
{
    _ice_connection_change_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::SetIceCandidateCallback(
    Callback<IceCandidate> &&callback) noexcept
{
    _ice_candidate_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::SetIceCandidatesRemovedCallback(
    Callback<CandidateListPtr> &&callback) noexcept
{
    _ice_candidates_removed_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::SetTrackAddedCallback(
    Callback<TransceiverPtr> &&callback) noexcept
{
    _track_added_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::OnRenegotiationNeeded()
{
    if(_renegotiation_needed_callback)
    {
        _renegotiation_needed_callback();
    }
}

void MediaServer::PeerConnectionObserver::OnIceGatheringChange(
    webrtc::PeerConnectionInterface::IceGatheringState new_state)
{
    if(_ice_gathering_state_changed_callback)
    {
        _ice_gathering_state_changed_callback(IceGatheringStateFrom(new_state));
    }
}

void MediaServer::PeerConnectionObserver::OnIceConnectionChange(
    webrtc::PeerConnectionInterface::IceConnectionState new_state)
{
    if(_ice_connection_change_callback)
    {
        _ice_connection_change_callback(IceConnectionStateFrom(new_state));
    }
}

void MediaServer::PeerConnectionObserver::OnIceCandidate(
    const webrtc::IceCandidateInterface *ice_candidate)
{
    if(_ice_candidate_callback)
    {
        // Parse sdp
        std::string sdp;
        if(!ice_candidate->ToString(&sdp))
        {
            RTC_LOG(LS_WARNING, "Ice candidate has no SDP, ignoring.");
            return;
        }
        Utils::StringHelper::EnsureNullTerminatedCString(sdp);

        // Parsse sdp_mid
        std::string sdp_mid = ice_candidate->sdp_mid();
        Utils::StringHelper::EnsureNullTerminatedCString(sdp_mid);

        // Invoke the callback
        _ice_candidate_callback(MediaServer::IceCandidate(
            sdp.c_str(), sdp_mid.c_str(), ice_candidate->sdp_mline_index));
    }
}

void MediaServer::PeerConnectionObserver::SetRenegotiationNeededCallback(
    Callback<> &&callback) noexcept
{
    _renegotiation_needed_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::OnTrack(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> transceiver)
{
    if(_track_added_callback)
    {
        _track_added_callback(transceiver.get());
    }
}