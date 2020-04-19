#include "pch.h"

#include "peer_connection_observer.h"
#include "peer_connection_observer_interop.h"

PeerConnectionObserverIntPtr CONVENTION PeerConnectionObserverCreate()
{
    return new Wrappers::PeerConnectionObserver();
}

void CONVENTION
PeerConnectionObserverDestroy(PeerConnectionObserverIntPtr peer_connection_observer_ptr)
{
    delete static_cast<Wrappers::PeerConnectionObserver *>(peer_connection_observer_ptr);
}

void CONVENTION PeerConnectionObserverSetRenegotiationNeededCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    RenegotiationNeededCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetRenegotiationNeededCallback(Callback<>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetRemoteTrackAddedCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    RemoteTrackAddedCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetRemoteTrackAddedCallback(
            Callback<RtpTransceiverInterfacePtr>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetRemoteTrackRemovedCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    RemoteTrackRemovedCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetRemoteTrackRemovedCallback(
            Callback<RtpReceiverInterfacePtr>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceGatheringStateChangedCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    IceGatheringStateChangedCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetIceGatheringStateChangedCallback(
            Callback<IceGatheringState>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceCandidatesRemovedCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    IceCandidatesRemovedCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetIceCandidatesRemovedCallback(
            Callback<CandidateListPtr>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceCandidateCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    IceCandidateCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetIceCandidateCallback(Callback<IceCandidate>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceConnectionChangeCallback(
    PeerConnectionObserverIntPtr peer_connection_observer_ptr,
    IceConnectionChangeCallback call_back,
    UserData user_data)
{
    StaticCastOrThrow<PeerConnectionObserver>(peer_connection_observer_ptr)
        ->SetIceConnectionChangeCallback(
            Callback<IceConnectionState>{std::move(call_back), user_data});
}
