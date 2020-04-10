#pragma once

#include "pch.h"

#include "media_server_ice_gathering_state.h"

using namespace MediaServer;

extern "C"
{
    using PeerConnectionObserverIntPtr = void *;
    using UserData = void *;
    using CandidateListPtr = const void *;
    using RtpTransceiverInterfacePtr = void *;
    using RtpReceiverInterfacePtr = void *;

    using RenegotiationNeededCallback = void(CONVENTION *)(UserData);
    using IceGatheringStateChangedCallback = void(CONVENTION *)(UserData, IceGatheringState);
    using IceConnectionChangeCallback = void(CONVENTION *)(UserData, IceConnectionState);
    using IceCandidateCallback = void(CONVENTION *)(UserData, IceCandidate);
    using IceCandidatesRemovedCallback = void(CONVENTION *)(UserData, CandidateListPtr);
    using RemoteTrackAddedCallback = void(CONVENTION *)(UserData, RtpTransceiverInterfacePtr);
    using RemoteTrackRemovedCallback = void(CONVENTION *)(UserData, RtpReceiverInterfacePtr);

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

    EXPORT void CONVENTION
    PeerConnectionObserverSetCallbacks(PeerConnectionObserverIntPtr peer_connection_observer_ptr,
                                       PeerConnectionObserverCallbacks call_backs,
                                       void *user_data);
}