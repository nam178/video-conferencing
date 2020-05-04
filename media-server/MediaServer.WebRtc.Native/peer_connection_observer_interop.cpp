#include "pch.h"

#include "peer_connection_observer.h"
#include "peer_connection_observer_interop.h"

Wrappers::PeerConnectionObserver *CONVENTION PeerConnectionObserverCreate()
{
    return new Wrappers::PeerConnectionObserver();
}

void CONVENTION
PeerConnectionObserverDestroy(Wrappers::PeerConnectionObserver *peer_connection_observer_ptr)
{
    delete peer_connection_observer_ptr;
}

void CONVENTION PeerConnectionObserverSetRenegotiationNeededCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    RenegotiationNeededCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetRenegotiationNeededCallback(
        Callback<>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetRemoteTrackAddedCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    RemoteTrackAddedCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetRemoteTrackAddedCallback(
        Callback<Wrappers::RtpReceiver *>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetRemoteTrackRemovedCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    RemoteTrackRemovedCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetRemoteTrackRemovedCallback(
        Callback<webrtc::RtpReceiverInterface *>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceGatheringStateChangedCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    IceGatheringStateChangedCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetIceGatheringStateChangedCallback(
        Callback<IceGatheringState>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceCandidatesRemovedCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    IceCandidatesRemovedCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetIceCandidatesRemovedCallback(
        Callback<const std::vector<cricket::Candidate> *>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceCandidateCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    IceCandidateCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetIceCandidateCallback(
        Callback<IceCandidate>{std::move(call_back), user_data});
}

void CONVENTION PeerConnectionObserverSetIceConnectionChangeCallback(
    Wrappers::PeerConnectionObserver *peer_connection_observer_ptr,
    IceConnectionChangeCallback call_back,
    UserData user_data)
{
    peer_connection_observer_ptr->SetIceConnectionChangeCallback(
        Callback<IceConnectionState>{std::move(call_back), user_data});
}
