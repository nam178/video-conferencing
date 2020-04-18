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

void CONVENTION
PeerConnectionObserverSetCallbacks(PeerConnectionObserverIntPtr peer_connection_observer_ptr,
                                   PeerConnectionObserverCallbacks call_backs,
                                   void *user_data)
{
    auto observer =
        static_cast<Wrappers::PeerConnectionObserver *>(peer_connection_observer_ptr);
    if(!observer)
    {
        throw new std::runtime_error("peer_connection_observer_ptr is null pointer");
    }

    observer->SetIceCandidateCallback(
        Callback<IceCandidate>{std::move(call_backs._ice_candidate_callback), user_data});
    observer->SetIceCandidatesRemovedCallback(Callback<CandidateListPtr>{
        std::move(call_backs._ice_candidates_removed_callback), user_data});
    observer->SetIceConnectionChangeCallback(Callback<IceConnectionState>{
        std::move(call_backs._ice_connection_change_callback), user_data});
    observer->SetIceGatheringStateChangedCallback(Callback<IceGatheringState>{
        std::move(call_backs._ice_gathering_state_changed_callback), user_data});
    observer->SetRemoteTrackAddedCallback(Callback<RtpTransceiverInterfacePtr>{
        std::move(call_backs._remote_track_added_callback), user_data});
    observer->SetRemoteTrackRemovedCallback(Callback<RtpReceiverInterfacePtr>{
        std::move(call_backs._remote_track_removed_callback), user_data});
    observer->SetRenegotiationNeededCallback(
        Callback<>{std::move(call_backs._renegotiation_needed_callback), user_data});
}
