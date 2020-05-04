#include "pch.h"

#include "passive_video_track.h"
#include "passive_video_track_source.h"
#include "peer_connection.h"
#include "peer_connection_factory.h"
#include "peer_connection_factory_interop.h"
#include "peer_connection_interop.h"
#include "video_track.h"

Wrappers::PeerConnectionFactory *CONVENTION PeerConnectionFactoryCreate()
{
    return new Wrappers::PeerConnectionFactory();
}

void CONVENTION PeerConnectionFactoryInitialize(Wrappers::PeerConnectionFactory *instance)
{
    instance->Initialize();
}

void CONVENTION PeerConnectionFactoryTearDown(Wrappers::PeerConnectionFactory *instance)
{
    instance->TearDown();
}

void CONVENTION PeerConnectionFactoryDestroy(Wrappers::PeerConnectionFactory *instance)
{
    if(instance)
    {
        delete static_cast<Wrappers::PeerConnectionFactory *>(instance);
    }
}

webrtc::PeerConnectionInterface::IceServers ConvertToIceServersList(IceServerConfig *ice_servers,
                                                                    int32_t ice_server_length)
{
    webrtc::PeerConnectionInterface::IceServers serverList{};

    for(size_t i = 0; i < ice_server_length; i++)
    {
        webrtc::PeerConnectionInterface::IceServer server{};

        if(ice_servers->_username)
            server.username = ice_servers->_username;
        if(ice_servers->_password)
            server.password = ice_servers->_password;
        if(!ice_servers->_comma_seperated_urls)
        {
            throw new std::runtime_error("_comma_seperated_urls is NULL");
        }

        std::stringstream string_stream(ice_servers->_comma_seperated_urls);
        std::string line;
        while(std::getline(string_stream, line, ';'))
        {
            server.urls.push_back(line);
        }
        ice_servers++;
    }
    return serverList;
}

EXPORT Wrappers::PeerConnection *CONVENTION PeerConnectionFactoryCreatePeerConnection(
    Wrappers::PeerConnectionFactory *peer_connection_factory,
    IceServerConfig *ice_servers,
    int32_t ice_server_length,
    Wrappers::PeerConnectionObserver *peer_connection_observer)
{
    webrtc::PeerConnectionInterface::RTCConfiguration rtc_config{};

    rtc_config.servers = ConvertToIceServersList(ice_servers, ice_server_length);
    rtc_config.enable_rtp_data_channel = false; // Always false for security
    rtc_config.enable_dtls_srtp = true;         // Always true for security
    rtc_config.type = webrtc::PeerConnectionInterface::IceTransportsType::kAll;
    rtc_config.bundle_policy =
        webrtc::PeerConnectionInterface::BundlePolicy::kBundlePolicyMaxCompat;
    rtc_config.sdp_semantics = webrtc::SdpSemantics::kUnifiedPlan;

    webrtc::PeerConnectionDependencies dependencies(peer_connection_observer);

    auto peer_connection =
        peer_connection_factory->GetPeerConnectionFactory()->CreatePeerConnection(
            rtc_config, std::move(dependencies));

    return new Wrappers::PeerConnection(std::move(peer_connection));
}

Wrappers::VideoTrack *CONVENTION
PeerConnectionFactoryCreateVideoTrack(Wrappers::PeerConnectionFactory *peer_connection_factory,
                                      Video::PassiveVideoTrackSource *passive_video_track_souce,
                                      const char *track_name)
{
    auto factory = peer_connection_factory->GetPeerConnectionFactory();
    if(!factory)
    {
        RTC_LOG(LS_ERROR, "peer_connection_factory is null");
        throw new std::runtime_error("peer_connection_factory is null ");
    }

    return new Wrappers::VideoTrack(
        factory->CreateVideoTrack(track_name, passive_video_track_souce));
}

Wrappers::RtcThread *PeerConnectionFactoryGetSignallingThread(
    Wrappers::PeerConnectionFactory *peer_connection_factory_ptr)
{
    return peer_connection_factory_ptr->GetSignallingThreadWrapper();
}
