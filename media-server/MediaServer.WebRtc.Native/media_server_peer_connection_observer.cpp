#include "pch.h"

#include "media_server_peer_connection_observer.h"
#include "utils_string_helper.h"

using namespace MediaServer;

IceGatheringState IceGatheringStateFrom(
    webrtc::PeerConnectionInterface::IceGatheringState ice_gathering_state)
{
    static_assert((int)IceGatheringState::New ==
                  (int)webrtc::PeerConnectionInterface::IceGatheringState::kIceGatheringNew);
    static_assert((int)IceGatheringState::Gathering ==
                  (int)webrtc::PeerConnectionInterface::IceGatheringState::kIceGatheringGathering);
    static_assert((int)IceGatheringState::Complete ==
                  (int)webrtc::PeerConnectionInterface::IceGatheringState::kIceGatheringComplete);
    return (IceGatheringState)ice_gathering_state;
}

IceConnectionState IceConnectionStateFrom(
    webrtc::PeerConnectionInterface::IceConnectionState ice_connection_state)
{
    static_assert((int)IceConnectionState::IceConnectionNew ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionNew);
    static_assert((int)IceConnectionState::IceConnectionChecking ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionChecking);
    static_assert((int)IceConnectionState::IceConnectionClosed ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionClosed);
    static_assert(
        (int)IceConnectionState::IceConnectionCompleted ==
        (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionCompleted);
    static_assert(
        (int)IceConnectionState::IceConnectionConnected ==
        (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionConnected);
    static_assert(
        (int)IceConnectionState::IceConnectionDisconnected ==
        (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionDisconnected);
    static_assert((int)IceConnectionState::IceConnectionFailed ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionFailed);
    static_assert((int)IceConnectionState::IceConnectionMax ==
                  (int)webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionMax);
    return (IceConnectionState)ice_connection_state;
}

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

void MediaServer::PeerConnectionObserver::SetRemoteTrackAddedCallback(
    Callback<RtpTransceiverInterfacePtr> &&callback) noexcept
{
    _remote_track_added_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::SetRemoteTrackRemovedCallback(
    Callback<RtpReceiverInterfacePtr> &&callback) noexcept
{
    _remote_track_removed_callback = std::move(callback);
}

void MediaServer::PeerConnectionObserver::OnSignalingChange(
    webrtc::PeerConnectionInterface::SignalingState new_state)
{
    RTC_LOG(LS_INFO, "signaling state changed: SignalingState(" + new_state + ")");
}

void MediaServer::PeerConnectionObserver::OnDataChannel(
    rtc::scoped_refptr<webrtc::DataChannelInterface> data_channel)
{
    RTC_LOG(LS_INFO, "data channel created");
}

MediaServer::PeerConnectionObserver::PeerConnectionObserver()
{
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
        _ice_candidate_callback(MediaServer::IceCandidate{
            sdp.c_str(), sdp_mid.c_str(), ice_candidate->sdp_mline_index()});
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
    if(_remote_track_added_callback)
    {
        _remote_track_added_callback(transceiver.get());
    }
}

void MediaServer::PeerConnectionObserver::OnRemoveTrack(
    rtc::scoped_refptr<webrtc::RtpReceiverInterface> receiver)
{
    if(_remote_track_removed_callback)
    {
        _remote_track_removed_callback(receiver.get());
    }
}

void MediaServer::PeerConnectionObserver::OnIceCandidatesRemoved(
    const std::vector<cricket::Candidate> &candidates)
{
    if(_ice_candidates_removed_callback)
    {
        _ice_candidates_removed_callback(&candidates);
    }
}