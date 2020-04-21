#pragma once

#include "pch.h"

#include "ice_gathering_state.h"

using namespace Wrappers;

extern "C"
{
    using PeerConnectionObserverIntPtr = void *;
    using UserData = void *;
    using CandidateListPtr = const void *;
    using RtpReceiverWrapperPtr = void *;
    using RtpReceiverPtr = void *;

    using RenegotiationNeededCallback = void(CONVENTION *)(UserData);
    using IceGatheringStateChangedCallback = void(CONVENTION *)(UserData, IceGatheringState);
    using IceConnectionChangeCallback = void(CONVENTION *)(UserData, IceConnectionState);
    using IceCandidateCallback = void(CONVENTION *)(UserData, IceCandidate);
    using IceCandidatesRemovedCallback = void(CONVENTION *)(UserData, CandidateListPtr);
    using RemoteTrackAddedCallback = void(CONVENTION *)(UserData, RtpReceiverWrapperPtr);
    using RemoteTrackRemovedCallback = void(CONVENTION *)(UserData, RtpReceiverPtr);

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

    EXPORT PeerConnectionObserverIntPtr CONVENTION PeerConnectionObserverCreate();

    EXPORT void CONVENTION
    PeerConnectionObserverDestroy(PeerConnectionObserverIntPtr peer_connection_observer_ptr);

    EXPORT void CONVENTION PeerConnectionObserverSetRenegotiationNeededCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        RenegotiationNeededCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceGatheringStateChangedCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        IceGatheringStateChangedCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceConnectionChangeCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        IceConnectionChangeCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceCandidateCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        IceCandidateCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetIceCandidatesRemovedCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        IceCandidatesRemovedCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetRemoteTrackAddedCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        RemoteTrackAddedCallback call_back,
        UserData user_data);
    EXPORT void CONVENTION PeerConnectionObserverSetRemoteTrackRemovedCallback(
        PeerConnectionObserverIntPtr peer_connection_observer_ptr,
        RemoteTrackRemovedCallback call_back,
        UserData user_data);
}