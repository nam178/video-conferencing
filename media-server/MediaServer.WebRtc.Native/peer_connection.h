#pragma once

#include "pch.h"

#include "create_sdp_result.h"
#include "rtp_sender.h"
#include "rtp_transceiver.h"

using Success = bool;
using ErrorMessage = const char *;

namespace Shim
{
// manages the smart pointer to the libWebRTC peer connection
// so it won't get destroyed
class PeerConnection final
{
  public:
    // Contruct the PeerConnection resource wrapper
    // from transceiver_entry managed pointer.
    // This PeerConnection then takes ownership of said pointer.
    PeerConnection(rtc::scoped_refptr<webrtc::PeerConnectionInterface> &&peer_connection_interface);

    // Create an offer, will be executed in signalling thread by the lib
    void CreateOffer(Callback<Shim::CreateSdpResult> &&callback);

    // Create answer to an sdp offer.
    // the callback will be called on the signalling thread in theory.
    // The callback owns the SessionDescriptionInterface*
    void CreateAnswer(Callback<Shim::CreateSdpResult> &&callback);

    // Completely kill this, implementation maps to native PeerConnection Close()
    void Close();

    // Add ice candiadate,
    // should only call ths after RemoveSessionDescription() is set
    bool AddIceCandiate(const char *sdp_mid,
                        int sdp_mline_index,
                        const char *sdp,
                        std::string &out_error);

    // Set remote session description
    void RemoteSessionDescription(const char *sdp_type,
                                  const char *sdp,
                                  Callback<Success, ErrorMessage> callback);

    // Set local session description
    // Must be called immediately after CreateAnswer()
    void LocalSessionDescription(const char *sdp_type,
                                 const char *sdp,
                                 Callback<Success, ErrorMessage> callback);

    // Add and remove local tracks,
    // and return the rtp sender associated with this track
    std::unique_ptr<Shim::RtpSender> AddTrack(
        rtc::scoped_refptr<webrtc::MediaStreamTrackInterface> track,
        const std::vector<std::string> &stream_ids);

    void RemoveTrack(Shim::RtpSender *rtp_sender);

    // get the raw pointer to the underlying native
    // PeerConnectionInterface
    webrtc::PeerConnectionInterface *GetPeerConnectionInterface();

    void Register(RtpTransceiver::CreateUnmanagedInstanceFuncPtr create_unmanaged_transceiver_fnptr)
    {
        if(create_unmanaged_transceiver_fnptr == nullptr)
        {
            throw new std::invalid_argument("Argument is nullptr");
        }
        std::scoped_lock(_mutex);
        _create_unmanaged_transceiver_fnptr = create_unmanaged_transceiver_fnptr;
    }

    struct TransceiverEntry
    {
        TransceiverEntry(Shim::RtpTransceiver *transceiver, void *unmanaged)
            : _rtpTransceiverShim(std::unique_ptr<Shim::RtpTransceiver>(transceiver))
        {
        }

        std::unique_ptr<Shim::RtpTransceiver> _rtpTransceiverShim;
        void *unmanaged;
    };

    void GetTransceivers()
    {
        // Get native transceivers and transceivers_index them.
        auto transceivers = _peer_connection_interface->GetTransceivers();
        std::unordered_set<webrtc::RtpTransceiverInterface *> transceivers_index{};
        for(auto &tmp : transceivers)
        {
            transceivers_index.insert(tmp.get());
        }

        // Transceiver should never be deleted,
        // We'll do one loop to checl
        for(auto &pair : _last_known_transceivers)
        {
            if(transceivers_index.count(pair.first) == 0)
            {
                RTC_LOG(LS_ERROR) << "Transceiver was deleted?";
                throw new std::runtime_error("Transceiver was deleted?");
            }
        }

        // Second loop to create new RtpTransceiver
        // This loop move/kill the smart pointers so do it after.
        for(auto &smart_ptr : transceivers)
        {
            auto transceiver_raw_ptr = smart_ptr.get();
            // This smart_ptr is new to us, create an entry for it
            if(_last_known_transceivers.count(transceiver_raw_ptr) == 0)
            {
                RtpTransceiver::CreateUnmanagedInstanceFuncPtr tmp;
                {
                    std::scoped_lock(_mutex);
                    tmp = _create_unmanaged_transceiver_fnptr;
                }
                if(!tmp)
                {
                    throw new std::runtime_error(
                        "Unmanaged function pointer to create transceiver has not been registered");
                }

                auto unmanaged_transceiver_ptr = new Shim::RtpTransceiver(std::move(smart_ptr));
                auto managed_transceiver_ptr = tmp(unmanaged_transceiver_ptr);

                auto transceiver_entry = std::make_unique<TransceiverEntry>(
                    unmanaged_transceiver_ptr, managed_transceiver_ptr);

                _last_known_transceivers.insert(
                    std::pair<webrtc::RtpTransceiverInterface *, std::unique_ptr<TransceiverEntry>>{
                        transceiver_raw_ptr, std::move(transceiver_entry)});
            }
        }
    }

  private:
    rtc::scoped_refptr<webrtc::PeerConnectionInterface> _peer_connection_interface;
    std::unordered_map<webrtc::RtpTransceiverInterface *, std::unique_ptr<TransceiverEntry>>
        _last_known_transceivers{};
    RtpTransceiver::CreateUnmanagedInstanceFuncPtr _create_unmanaged_transceiver_fnptr = nullptr;
    std::mutex _mutex{};
};
} // namespace Shim