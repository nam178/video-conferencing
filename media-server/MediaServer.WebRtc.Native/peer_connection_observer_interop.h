#pragma once

#include "pch.h"

#include "ice_gathering_state.h"

using namespace Shim;

extern "C"
{
    using UserData = void *;

    using RenegotiationNeededCallback = void(CONVENTION *)(UserData);
    using IceGatheringStateChangedCallback = void(CONVENTION *)(UserData, IceGatheringState);
    using IceConnectionChangeCallback = void(CONVENTION *)(UserData, IceConnectionState);
    using IceCandidateCallback = void(CONVENTION *)(UserData, IceCandidate);
    using IceCandidatesRemovedCallback =
        void(CONVENTION *)(UserData, const std::vector<cricket::Candidate> *);
    using RemoteTrackAddedCallback = void(CONVENTION *)(UserData, Shim::RtpTransceiver *);
    using RemoteTrackRemovedCallback = void(CONVENTION *)(UserData, webrtc::RtpReceiverInterface *);

    struct PeerConnectionObserverCallbacks
    {
        RenegotiationNeededCallback _renegotiation_needed_callback{};
        IceGatheringStateChangedCallback _ice_gathering_state_changed_callback{};
        IceConnectionChangeCallback _ice_connection_change_callback{};
        IceCandidateCallback _ice_candidate_callback{};
        IceCandidatesRemovedCallback _ice_candidates_removed_callback{};
        RemoteTrackAddedCallback _remote_track_added_callback{};
        RemoteTrackRemovedCallback _remote_track_removed_callback{};
    };

    EXPORT Shim::PeerConnectionObserver *CONVENTION PeerConnectionObserverCreate();

    EXPORT void CONVENTION
    PeerConnectionObserverDestroy(Shim::PeerConnectionObserver *peer_connection_observer_ptr);

    EXPORT void CONVENTION PeerConnectionObserverSetRenegotiationNeededCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        RenegotiationNeededCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceGatheringStateChangedCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        IceGatheringStateChangedCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceConnectionChangeCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        IceConnectionChangeCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceCandidateCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        IceCandidateCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceCandidatesRemovedCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        IceCandidatesRemovedCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetRemoteTrackAddedCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        RemoteTrackAddedCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetRemoteTrackRemovedCallback(
        Shim::PeerConnectionObserver *peer_connection_observer_ptr,
        RemoteTrackRemovedCallback call_back,
        UserData user_data);
}