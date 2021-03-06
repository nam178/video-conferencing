#include "pch.h"

#include "peer_connection_observer.h"
#include "string_helper.h"

using namespace Shim;

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

void Shim::PeerConnectionObserver::SetIceGatheringStateChangedCallback(
    Callback<IceGatheringState> &&callback) noexcept
{
    _ice_gathering_state_changed_callback = std::move(callback);
}

void Shim::PeerConnectionObserver::SetIceConnectionChangeCallback(
    Callback<IceConnectionState> &&callback) noexcept
{
    _ice_connection_change_callback = std::move(callback);
}

void Shim::PeerConnectionObserver::SetIceCandidateCallback(
    Callback<IceCandidate> &&callback) noexcept
{
    _ice_candidate_callback = std::move(callback);
}

void Shim::PeerConnectionObserver::SetIceCandidatesRemovedCallback(
    Callback<const std::vector<cricket::Candidate> *> &&callback) noexcept
{
    _ice_candidates_removed_callback = std::move(callback);
}

void Shim::PeerConnectionObserver::SetRemoteTrackAddedCallback(
    Callback<Shim::RtpTransceiver *> &&callback) noexcept
{
    _remote_track_added_callback = std::move(callback);
}

void Shim::PeerConnectionObserver::SetRemoteTrackRemovedCallback(
    Callback<webrtc::RtpReceiverInterface *> &&callback) noexcept
{
    _remote_track_removed_callback = std::move(callback);
}

void Shim::PeerConnectionObserver::OnSignalingChange(
    webrtc::PeerConnectionInterface::SignalingState new_state)
{
    RTC_LOG(LS_INFO) << "signaling state changed: SignalingState(" << new_state << ")";
}

void Shim::PeerConnectionObserver::OnDataChannel(
    rtc::scoped_refptr<webrtc::DataChannelInterface> data_channel)
{
    RTC_LOG(LS_INFO) << "data channel created";
}

Shim::PeerConnectionObserver::PeerConnectionObserver()
{
}

void Shim::PeerConnectionObserver::OnRenegotiationNeeded()
{
    if(_renegotiation_needed_callback)
    {
        _renegotiation_needed_callback();
    }
}

void Shim::PeerConnectionObserver::OnIceGatheringChange(
    webrtc::PeerConnectionInterface::IceGatheringState new_state)
{
    if(_ice_gathering_state_changed_callback)
    {
        _ice_gathering_state_changed_callback(IceGatheringStateFrom(new_state));
    }
}

void Shim::PeerConnectionObserver::OnIceConnectionChange(
    webrtc::PeerConnectionInterface::IceConnectionState new_state)
{
    if(_ice_connection_change_callback)
    {
        _ice_connection_change_callback(IceConnectionStateFrom(new_state));
    }
}

void Shim::PeerConnectionObserver::OnIceCandidate(
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
        // Notes that the char* will be deleted by the parent
        // string above
        _ice_candidate_callback(
            Shim::IceCandidate{sdp.c_str(), sdp_mid.c_str(), ice_candidate->sdp_mline_index()});
    }
}

void Shim::PeerConnectionObserver::SetRenegotiationNeededCallback(
    Callback<> &&callback) noexcept
{
    _renegotiation_needed_callback = std::move(callback);
}

bool Contains(std::vector<Shim::RtpReceiver *> collection,
              rtc::scoped_refptr<webrtc::RtpReceiverInterface> value)
{
    return collection.end() !=
           std::find_if(
               collection.begin(), collection.end(), [value](const Shim::RtpReceiver *val) {
                   return val->GetRtpReceiverInterface() == value.get();
               });
}

void Shim::PeerConnectionObserver::OnTrack(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> transceiver)
{
    if(_remote_track_added_callback)
    {
        auto tmp = new Shim::RtpTransceiver(std::move(transceiver));
        _remote_track_added_callback(tmp);
    }
}

void Shim::PeerConnectionObserver::OnRemoveTrack(
    rtc::scoped_refptr<webrtc::RtpReceiverInterface> receiver)
{
    if(_remote_track_removed_callback)
    {
        _remote_track_removed_callback(receiver.get());
    }
}

void Shim::PeerConnectionObserver::OnIceCandidatesRemoved(
    const std::vector<cricket::Candidate> &candidates)
{
    if(_ice_candidates_removed_callback)
    {
        _ice_candidates_removed_callback(&candidates);
    }
}