#pragma once

#include "pch.h"

extern "C"
{
    using PeerConnectionFactoryPtr = void *;
    using PeerConnectionRawPointer = void *;
    using PeerConnectionObserverRawPointer = void *;
    using PassiveVideoTrackPtr = void *;
    using PassiveVideoTrackSourcePtr = void *;

    struct IceServerConfig
    {
        const char *_comma_seperated_urls{};
        const char *_username{};
        const char *_password{};
    };

    // Create new isntance of PeerConnectionFactory
    EXPORT PeerConnectionFactoryPtr CONVENTION PeerConnectionFactoryManagerCreate();

    // Call Initialize() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION
    PeerConnectionFactoryManagerInitialize(PeerConnectionFactoryPtr instance);

    // Call TearDown() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION
    PeerConnectionFactoryManagerTearDown(PeerConnectionFactoryPtr instance);

    // Release/delete the memory occupied by an instance of PeerConnectionFactory
    // Make sure you call TearDown() first
    EXPORT void CONVENTION
    PeerConnectionFactoryManagerDestroy(PeerConnectionFactoryPtr instance);

    EXPORT PeerConnectionRawPointer CONVENTION PeerConnectionFactoryCreatePeerConnection(
        PeerConnectionFactoryPtr peer_connection_factory,
        IceServerConfig *ice_servers,
        int32_t ice_server_length,
        PeerConnectionObserverRawPointer peer_connection_observer);

    EXPORT PassiveVideoTrackPtr CONVENTION PeerConnectionFactoryCreatePassiveVideoTrack(
        PeerConnectionFactoryPtr peer_connection_factory,
        PassiveVideoTrackSourcePtr passive_video_track_souce_ptr,
        const char *track_name);
}