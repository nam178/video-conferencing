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
    EXPORT Wrappers::PeerConnectionFactory *CONVENTION PeerConnectionFactoryCreate();

    // Call Initialize() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION
    PeerConnectionFactoryInitialize(Wrappers::PeerConnectionFactory *instance);

    // Call TearDown() on an instance of PeerConnectionFactory
    EXPORT void CONVENTION PeerConnectionFactoryTearDown(Wrappers::PeerConnectionFactory *instance);

    // Release/delete the memory occupied by an instance of PeerConnectionFactory
    // Make sure you call TearDown() first
    EXPORT void CONVENTION PeerConnectionFactoryDestroy(Wrappers::PeerConnectionFactory *instance);

    EXPORT Wrappers::PeerConnection *CONVENTION PeerConnectionFactoryCreatePeerConnection(
        Wrappers::PeerConnectionFactory *peer_connection_factory,
        IceServerConfig *ice_servers,
        int32_t ice_server_length,
        Wrappers::PeerConnectionObserver *peer_connection_observer);

    EXPORT Wrappers::VideoTrack *CONVENTION
    PeerConnectionFactoryCreateVideoTrack(Wrappers::PeerConnectionFactory *peer_connection_factory,
                                          Video::PassiveVideoTrackSource *passive_video_track_souce,
                                          const char *track_name);

    EXPORT Wrappers::RtcThread *PeerConnectionFactoryGetSignallingThread(
        Wrappers::PeerConnectionFactory *peer_connection_factory);
}