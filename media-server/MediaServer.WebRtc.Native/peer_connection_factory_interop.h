#pragma once

#include "pch.h"

#include "passive_video_track_source.h"
#include "peer_connection_factory.h"
#include "peer_connection_observer.h"
#include "video_track.h"

extern "C"
{
    struct IceServerConfig
    {
        const char *_comma_seperated_urls{};
        const char *_username{};
        const char *_password{};
    };

    // Create new isntance of PeerConnectionFactory
    EXPORT Shim::PeerConnectionFactory *CONVENTION PeerConnectionFactoryCreate();

    // Call Initialize() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION
    PeerConnectionFactoryInitialize(Shim::PeerConnectionFactory *instance);

    // Call TearDown() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION PeerConnectionFactoryTearDown(Shim::PeerConnectionFactory *instance);

    // Release/delete the memory occupied by an instance of PeerConnectionFactory
    // Make sure you call TearDown() first
    EXPORT void CONVENTION PeerConnectionFactoryDestroy(Shim::PeerConnectionFactory *instance);

    EXPORT Shim::PeerConnection *CONVENTION PeerConnectionFactoryCreatePeerConnection(
        Shim::PeerConnectionFactory *peer_connection_factory,
        IceServerConfig *ice_servers,
        int32_t ice_server_length,
        Shim::PeerConnectionObserver *peer_connection_observer);

    EXPORT Shim::VideoTrack *CONVENTION
    PeerConnectionFactoryCreateVideoTrack(Shim::PeerConnectionFactory *peer_connection_factory,
                                          Video::PassiveVideoTrackSource *passive_video_track_souce,
                                          const char *track_name);

    EXPORT Shim::RtcThread *PeerConnectionFactoryGetSignallingThread(
        Shim::PeerConnectionFactory *peer_connection_factory);
}