#include "pch.h"

#include "media_server_peer_connection.h"
#include "media_server_peer_connection_factory.h"
#include "media_server_peer_connection_factory_interop.h"
#include "media_server_peer_connection_interop.h"

MediaServer::PeerConnectionFactoryManager *Cast(void *opaque_ptr)
{
    auto tmp = static_cast<MediaServer::PeerConnectionFactoryManager *>(opaque_ptr);
    if(!tmp)
    {
        RTC_LOG(LS_ERROR, "PeerConnectionFactoryManager null ptr");
        throw new std::runtime_error("PeerConnectionFactoryManager is null");
    }
    return tmp;
}

PeerConnectionFactoryPtr CONVENTION PeerConnectionFactoryManagerCreate()
{
    return new MediaServer::PeerConnectionFactoryManager();
}

PeerConnectionFactoryPtr CONVENTION
PeerConnectionFactoryManagerInitialize(PeerConnectionFactoryPtr instance)
{
    Cast(instance)->Initialize();
}

PeerConnectionFactoryPtr CONVENTION
PeerConnectionFactoryManagerTearDown(PeerConnectionFactoryPtr instance)
{
    Cast(instance)->TearDown();
}

PeerConnectionFactoryPtr CONVENTION
PeerConnectionFactoryManagerDestroy(PeerConnectionFactoryPtr instance)
{
    if(instance)
    {
        delete Cast(instance);
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

EXPORT PeerConnectionRawPointer CONVENTION
PeerConnectionFactoryCreatePeerConnection(PeerConnectionFactoryPtr peer_connection_factory,
                                          IceServerConfig *ice_servers,
                                          int32_t ice_server_length,
                                          PeerConnectionObserverRawPointer peer_connection_observer)
{
    webrtc::PeerConnectionInterface::RTCConfiguration rtc_config{};

    rtc_config.servers = ConvertToIceServersList(ice_servers, ice_server_length);
    rtc_config.enable_rtp_data_channel = false; // Always false for security
    rtc_config.enable_dtls_srtp = true;         // Always true for security
    rtc_config.type = webrtc::PeerConnectionInterface::IceTransportsType::kAll;
    rtc_config.bundle_policy =
        webrtc::PeerConnectionInterface::BundlePolicy::kBundlePolicyMaxCompat;
    rtc_config.sdp_semantics = webrtc::SdpSemantics::kUnifiedPlan;

    auto observer = static_cast<webrtc::PeerConnectionObserver *>(peer_connection_observer);
    if(!observer)
    {
        RTC_LOG(LS_ERROR, "peer_connection_observer is null");
        throw new std::runtime_error("peer_connection_observer is null");
    }

    auto factory_manager =
        static_cast<MediaServer::PeerConnectionFactoryManager *>(peer_connection_factory);
    if(!factory_manager)
    {
        RTC_LOG(LS_ERROR, "peer_connection_observer is null");
        throw new std::runtime_error("peer_connection_observer is null");
    }

    webrtc::PeerConnectionDependencies dependencies(observer);

    auto peer_connection = factory_manager->GetPeerConnectionFactory()->CreatePeerConnection(
        rtc_config, std::move(dependencies));

    return new MediaServer::PeerConnection(std::move(peer_connection));
}
