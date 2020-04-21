#include "pch.h"

#include "peer_connection_observer.h"
#include "string_helper.h"

using namespace Wrappers;

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

void Wrappers::PeerConnectionObserver::SetIceGatheringStateChangedCallback(
    Callback<IceGatheringState> &&callback) noexcept
{
    _ice_gathering_state_changed_callback = std::move(callback);
}

void Wrappers::PeerConnectionObserver::SetIceConnectionChangeCallback(
    Callback<IceConnectionState> &&callback) noexcept
{
    _ice_connection_change_callback = std::move(callback);
}

void Wrappers::PeerConnectionObserver::SetIceCandidateCallback(
    Callback<IceCandidate> &&callback) noexcept
{
    _ice_candidate_callback = std::move(callback);
}

void Wrappers::PeerConnectionObserver::SetIceCandidatesRemovedCallback(
    Callback<CandidateListPtr> &&callback) noexcept
{
    _ice_candidates_removed_callback = std::move(callback);
}

void Wrappers::PeerConnectionObserver::SetRemoteTrackAddedCallback(
    Callback<RtpReceiverPtr> &&callback) noexcept
{
    _remote_track_added_callback = std::move(callback);
}

void Wrappers::PeerConnectionObserver::SetRemoteTrackRemovedCallback(
    Callback<RtpReceiverPtr> &&callback) noexcept
{
    _remote_track_removed_callback = std::move(callback);
}

void Wrappers::PeerConnectionObserver::OnSignalingChange(
    webrtc::PeerConnectionInterface::SignalingState new_state)
{
    RTC_LOG(LS_INFO) << "signaling state changed: SignalingState(" << new_state << ")";
}

void Wrappers::PeerConnectionObserver::OnDataChannel(
    rtc::scoped_refptr<webrtc::DataChannelInterface> data_channel)
{
    RTC_LOG(LS_INFO) << "data channel created";
}

Wrappers::PeerConnectionObserver::PeerConnectionObserver()
{
}

void Wrappers::PeerConnectionObserver::OnRenegotiationNeeded()
{
    if(_renegotiation_needed_callback)
    {
        _renegotiation_needed_callback();
    }
}

void Wrappers::PeerConnectionObserver::OnIceGatheringChange(
    webrtc::PeerConnectionInterface::IceGatheringState new_state)
{
    if(_ice_gathering_state_changed_callback)
    {
        _ice_gathering_state_changed_callback(IceGatheringStateFrom(new_state));
    }
}

void Wrappers::PeerConnectionObserver::OnIceConnectionChange(
    webrtc::PeerConnectionInterface::IceConnectionState new_state)
{
    if(_ice_connection_change_callback)
    {
        _ice_connection_change_callback(IceConnectionStateFrom(new_state));
    }
}

void Wrappers::PeerConnectionObserver::OnIceCandidate(
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
            Wrappers::IceCandidate{sdp.c_str(), sdp_mid.c_str(), ice_candidate->sdp_mline_index()});
    }
}

void Wrappers::PeerConnectionObserver::SetRenegotiationNeededCallback(
    Callback<> &&callback) noexcept
{
    _renegotiation_needed_callback = std::move(callback);
}

bool Contains(std::vector<Wrappers::RtpReceiver *> collection,
              rtc::scoped_refptr<webrtc::RtpReceiverInterface> value)
{
    return collection.end() !=
           std::find_if(
               collection.begin(), collection.end(), [value](const Wrappers::RtpReceiver *val) {
                   return val->GetRtpReceiverInterface() == value.get();
               });
}

void Wrappers::PeerConnectionObserver::OnTrack(
    rtc::scoped_refptr<webrtc::RtpTransceiverInterface> transceiver)
{
    std::scoped_lock(_rtp_receivers_lock);

    // Just to be sure,
    // only handle this event if this transceiver is new to us
    if(!Contains(_rtp_receivers, transceiver->receiver()))
    {
        // Wrap and take ownership of the wrapper
        auto wrapper = new Wrappers::RtpReceiver(transceiver->receiver());
        _rtp_receivers.push_back(wrapper);

        // Expose the wrapper to unmanaged code
        if(_remote_track_added_callback)
        {
            _remote_track_added_callback(wrapper);
        }
    }
}

void Wrappers::PeerConnectionObserver::OnRemoveTrack(
    rtc::scoped_refptr<webrtc::RtpReceiverInterface> receiver)
{
    std::unique_ptr<Wrappers::RtpReceiver> match_result{};

    {
        std::scoped_lock(_rtp_receivers_lock);
        if(Contains(_rtp_receivers, receiver))
        {
            auto match_position =
                std::find_if(_rtp_receivers.begin(),
                             _rtp_receivers.end(),
                             [receiver](const Wrappers::RtpReceiver *value) {
                                 return value->GetRtpReceiverInterface() == receiver.get();
                             });
            match_result.reset(*match_position);
            _rtp_receivers.erase(match_position);
        }
    } // _rtp_receivers_lock ends

    if(_remote_track_removed_callback && match_result)
    {
        _remote_track_removed_callback(match_result.get());
    }
}

void Wrappers::PeerConnectionObserver::OnIceCandidatesRemoved(
    const std::vector<cricket::Candidate> &candidates)
{
    if(_ice_candidates_removed_callback)
    {
        _ice_candidates_removed_callback(&candidates);
    }
}

Wrappers::PeerConnectionObserver::~PeerConnectionObserver()
{
    // TODO make sure this gets called
    std::scoped_lock(_rtp_receivers_lock);

    for(auto tmp : _rtp_receivers)
    {
        delete tmp;
    }
}