#include "pch.h"

#include "media_server_create_session_description_observer.h"
#include "media_server_peer_connection.h"
#include "utils_string_helper.h"

MediaServer::PeerConnection::PeerConnection(
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface)
    : _peer_connection_interface(std::move(peer_connection_interface))
{
}

void MediaServer::PeerConnection::CreateAnswer(Callback<MediaServer::CreateAnswerResult> &&callback)
{
    const webrtc::PeerConnectionInterface::RTCOfferAnswerOptions opts{};

    _peer_connection_interface->CreateAnswer(
        new MediaServer::CreateSessionDescriptionObserver(
            [callback](Result<webrtc::SessionDescriptionInterface *> result) {
                if(result._success) 
                {
                    // Get sdp_type
                    auto sdp_type_str_ptr = webrtc::SdpTypeToString(result._result->GetType());
                    auto sdp_type_str = std::string(sdp_type_str_ptr);
                    Utils::StringHelper::EnsureNullTerminatedCString(sdp_type_str);

                    // Get sdp
                    std::string sdp_str;
                    if(result._result->ToString(&sdp_str))
                    {
                        callback({
                            false,
                            "Failed converting sdp to string",
                            nullptr,
                            nullptr,
                        });
                    }
                    Utils::StringHelper::EnsureNullTerminatedCString(sdp_str);

                     // Callback
                    callback({true, nullptr, sdp_type_str.c_str(), sdp_str.c_str()});
                }
                else
                {
                    callback({
                        false,
                        result._error_message.c_str(),
                        nullptr,
                        nullptr,
                    });
                }
            }),
        opts);
}

void MediaServer::PeerConnection::Close()
{
    _peer_connection_interface->Close();
}

webrtc::PeerConnectionInterface *MediaServer::PeerConnection::GetPeerConnectionInterface()
{
    return _peer_connection_interface.get();
}
