#include "pch.h"

#include "create_session_description_observer.h"
#include "peer_connection.h"
#include "set_session_description_observer.h"
#include "string_helper.h"

using SetFunction =
    void (webrtc::PeerConnectionInterface::*)(webrtc::SetSessionDescriptionObserver *observer,
                                              webrtc::SessionDescriptionInterface *);

std::function<void(Result<webrtc::SessionDescriptionInterface *>)> ConvertCallbackToLambda(
    Shim::Callback<Shim::CreateSdpResult> &&callback)
{
    return [callback{std::move(callback)}](Result<webrtc::SessionDescriptionInterface *> result) {
        if(result._success)
        {
            // Get sdp_type
            auto sdp_type_str_ptr = webrtc::SdpTypeToString(result._result->GetType());
            auto sdp_type_str = std::string(sdp_type_str_ptr);
            Utils::StringHelper::EnsureNullTerminatedCString(sdp_type_str);

            // Get sdp
            std::string sdp_str;
            if(!result._result->ToString(&sdp_str))
            {
                callback({
                    false,
                    "Failed converting sdp to string",
                    nullptr,
                    nullptr,
                });
                return;
            }
            Utils::StringHelper::EnsureNullTerminatedCString(sdp_str);

            // CreateSdpCallback
            callback({true, nullptr, sdp_type_str.c_str(), sdp_str.c_str()});
        }
        else
        {
            std::string error_message(result._error_message);
            Utils::StringHelper::EnsureNullTerminatedCString(error_message);
            callback({
                false,
                error_message.c_str(),
                nullptr,
                nullptr,
            });
        }
    };
}

void InvokeMethod(rtc::scoped_refptr<webrtc::PeerConnectionInterface> peer_connection_interface,
                  SetFunction function,
                  const char *sdp_type,
                  const char *sdp,
                  Shim::Callback<Success, ErrorMessage> callback)
{

    auto sdp_type_enum = webrtc::SdpTypeFromString(sdp_type);
    if(!sdp_type_enum.has_value())
    {
        std::string err = "Failed parsing sdp_type";
        Utils::StringHelper::EnsureNullTerminatedCString(err);
        callback(false, err.c_str());
        return;
    }

    auto callback_lambda = [callback](bool success, const char *error_message) {
        if(error_message)
        {
            std::string error_message(error_message);
            Utils::StringHelper::EnsureNullTerminatedCString(error_message);
            callback(false, error_message.c_str());
        }
        else
        {
            callback(true, nullptr);
        }
    };

    auto observer = new Shim::SetRemoteSessionDescriptionObserver(std::move(callback_lambda));
    auto session_description =
        webrtc::CreateSessionDescription(sdp_type_enum.value(), sdp).release();

    (peer_connection_interface.get()->*function)(observer, session_description);
}

Shim::PeerConnection::PeerConnection(
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface)
    : _peer_connection_interface(std::move(peer_connection_interface))
{
}

void Shim::PeerConnection::CreateOffer(Callback<Shim::CreateSdpResult> &&callback)
{
    const webrtc::PeerConnectionInterface::RTCOfferAnswerOptions opts{};

    // Notes
    // By looking at the source,
    // PeerConnectionInterface takes owner ship of CreateSessionDescriptionObserver
    // see struct CreateSessionDescriptionRequest in the source.
    _peer_connection_interface->CreateOffer(new Shim::CreateSessionDescriptionObserver(
                                                ConvertCallbackToLambda(std::move(callback))),
                                            opts);
}

void Shim::PeerConnection::CreateAnswer(Callback<Shim::CreateSdpResult> &&callback)
{
    const webrtc::PeerConnectionInterface::RTCOfferAnswerOptions opts{};

    // Notes
    // By looking at the source,
    // PeerConnectionInterface takes owner ship of CreateSessionDescriptionObserver
    // see struct CreateSessionDescriptionRequest in the source.
    _peer_connection_interface->CreateAnswer(new Shim::CreateSessionDescriptionObserver(
                                                 ConvertCallbackToLambda(std::move(callback))),
                                             opts);
}

void Shim::PeerConnection::Close()
{
    _peer_connection_interface->Close();
}

bool Shim::PeerConnection::AddIceCandiate(const char *sdp_mid,
                                              int sdp_mline_index,
                                              const char *sdp,
                                              std::string &out_error)
{
    webrtc::SdpParseError parse_error{};
    auto ice_candidate = webrtc::CreateIceCandidate(sdp_mid, sdp_mline_index, sdp, &parse_error);
    if(!ice_candidate)
    {
        out_error = "Line=" + parse_error.line + ", Description=" + parse_error.description;
        return false;
    }

    _peer_connection_interface->AddIceCandidate(ice_candidate);
}

void Shim::PeerConnection::RemoteSessionDescription(const char *sdp_type,
                                                        const char *sdp,
                                                        Callback<Success, ErrorMessage> callback)
{
    InvokeMethod(_peer_connection_interface,
                 &webrtc::PeerConnectionInterface::SetRemoteDescription,
                 sdp_type,
                 sdp,
                 callback);
}

void Shim::PeerConnection::LocalSessionDescription(const char *sdp_type,
                                                       const char *sdp,
                                                       Callback<Success, ErrorMessage> callback)
{
    InvokeMethod(_peer_connection_interface,
                 &webrtc::PeerConnectionInterface::SetLocalDescription,
                 sdp_type,
                 sdp,
                 callback);
}

std::unique_ptr<Shim::RtpSender> Shim::PeerConnection::AddTrack(
    rtc::scoped_refptr<webrtc::MediaStreamTrackInterface> track,
    const std::vector<std::string> &stream_ids)
{

    std::unique_ptr<Shim::RtpSender> result{};
    auto add_track_result = _peer_connection_interface->AddTrack(track, stream_ids);
    if(!add_track_result.ok())
    {
        RTC_LOG(LS_ERROR) << "Failed adding track into PeerConnection";
        return result;
    }
    result.reset(new Shim::RtpSender(std::move(add_track_result.value())));
    return result;
}

void Shim::PeerConnection::RemoveTrack(Shim::RtpSender *rtp_sender)
{
    if(!rtp_sender)
    {
        RTC_LOG(LS_ERROR) << "rtp_sender is nullptr";
        throw new std::runtime_error("rtp_sender is nullptr");
    }

    if(false == _peer_connection_interface->RemoveTrack(rtp_sender->Native()))
    {
        RTC_LOG(LS_ERROR) << "Failed removing track id=" << rtp_sender->Native()->track()->id();
    }
}

webrtc::PeerConnectionInterface *Shim::PeerConnection::GetPeerConnectionInterface()
{
    return _peer_connection_interface.get();
}
