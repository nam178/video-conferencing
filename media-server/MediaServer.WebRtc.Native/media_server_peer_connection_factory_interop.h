#pragma once

#include "pch.h"

extern "C"
{
    using PeerConnectionFactoryPtr = void *;
    using PeerConnectionRawPointer = void *;
    using PeerConnectionObserverRawPointer = void *;

    struct IceServerConfig
    {
        const char *_comma_seperated_urls{};
        const char *_username{};
        const char *_password{};
    };

    // Create new isntance of PeerConnectionFactoryManager
    EXPORT PeerConnectionFactoryPtr CONVENTION PeerConnectionFactoryManagerCreate();

    // Call Initialize() on an instance of PeerConnectionFactoryManager
    EXPORT PeerConnectionFactoryPtr CONVENTION
    PeerConnectionFactoryManagerInitialize(PeerConnectionFactoryPtr instance);

    // Call TearDown() on an instance of PeerConnectionFactoryManager
    EXPORT PeerConnectionFactoryPtr CONVENTION
    PeerConnectionFactoryManagerTearDown(PeerConnectionFactoryPtr instance);

    // Release/delete the memory occupied by an instance of PeerConnectionFactoryManager
    // Make sure you call TearDown() first
    EXPORT PeerConnectionFactoryPtr CONVENTION
    PeerConnectionFactoryManagerDestroy(PeerConnectionFactoryPtr instance);

    EXPORT PeerConnectionRawPointer CONVENTION PeerConnectionFactoryCreatePeerConnection(
        PeerConnectionFactoryPtr peer_connection_factory,
        IceServerConfig *ice_servers,
        int32_t ice_server_length,
        PeerConnectionObserverRawPointer peer_connection_observer);
}