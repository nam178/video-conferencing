#include "pch.h"

#include "create_session_description_observer.h"
#include "peer_connection.h"
#include "string_helper.h"

Wrappers::PeerConnection::PeerConnection(
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface)
    : _peer_connection_interface(std::move(peer_connection_interface))
{
}

void Wrappers::PeerConnection::CreateAnswer(Callback<Wrappers::CreateAnswerResult> &&callback)
{
    const webrtc::PeerConnectionInterface::RTCOfferAnswerOptions opts{};

    // Notes
    // By looking at the source,
    // PeerConnectionInterface takes owner ship of CreateSessionDescriptionObserver
    // see struct CreateSessionDescriptionRequest in the source.
    _peer_connection_interface->CreateAnswer(
        new Wrappers::CreateSessionDescriptionObserver(
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

                    // CreateAnswerCallback
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

void Wrappers::PeerConnection::Close()
{
    _peer_connection_interface->Close();
}

void Wrappers::PeerConnection::RemoteSessionDescription(const char *sdp_type, const char *sdp)
{
    rtc::scoped_refptr<webrtc::SetRemoteDescriptionObserverInterface> observer;

    auto sdp_type_enum = webrtc::SdpTypeFromString(sdp_type);
    if(!sdp_type_enum.has_value())
    {
        // TODO callback with error
        return;
    }

    auto session_description = webrtc::CreateSessionDescription(sdp_type_enum.value(), sdp);

    _peer_connection_interface->SetRemoteDescription(std::move(session_description), observer);
}

webrtc::PeerConnectionInterface *Wrappers::PeerConnection::GetPeerConnectionInterface()
{
    return _peer_connection_interface.get();
}
