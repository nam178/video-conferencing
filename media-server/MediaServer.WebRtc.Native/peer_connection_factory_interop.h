#pragma once

#include "pch.h"

#include "video_track.h"

extern "C"
{
    using PeerConnectionFactoryPtr = void *;
    using PeerConnectionObserverRawPointer = void *;
    using PassiveVideoTrackSourcePtr = void *;

    struct IceServerConfig
    {
        const char *_comma_seperated_urls{};
        const char *_username{};
        const char *_password{};
    };

    // Create new isntance of PeerConnectionFactory
    EXPORT Wrappers::PeerConnectionFactory *CONVENTION PeerConnectionFactoryCreate();

    // Call Initialize() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION PeerConnectionFactoryInitialize(PeerConnectionFactoryPtr instance);

    // Call TearDown() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION PeerConnectionFactoryTearDown(PeerConnectionFactoryPtr instance);

    // Release/delete the memory occupied by an instance of PeerConnectionFactory
    // Make sure you call TearDown() first
    EXPORT void CONVENTION PeerConnectionFactoryDestroy(PeerConnectionFactoryPtr instance);

    EXPORT Wrappers::PeerConnection *CONVENTION PeerConnectionFactoryCreatePeerConnection(
        PeerConnectionFactoryPtr peer_connection_factory,
        IceServerConfig *ice_servers,
        int32_t ice_server_length,
        PeerConnectionObserverRawPointer peer_connection_observer);

    EXPORT Wrappers::VideoTrack *CONVENTION
    PeerConnectionFactoryCreateVideoTrack(PeerConnectionFactoryPtr peer_connection_factory,
                                          PassiveVideoTrackSourcePtr passive_video_track_souce_ptr,
                                          const char *track_name);

    EXPORT Wrappers::RtcThread *PeerConnectionFactoryGetSignallingThread(
        PeerConnectionFactoryPtr peer_connection_factory_ptr);
}