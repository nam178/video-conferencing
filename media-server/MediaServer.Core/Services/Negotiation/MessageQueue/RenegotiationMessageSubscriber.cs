using MediaServer.Common.Patterns;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class RenegotiationMessageSubscriber : IMessageSubscriber
    {
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is RenegotiationMessage;

        public void Handle(Message message, Observer completionCallback)
        {
            var peerConnection = message.PeerConnection;
            _logger.Debug($"[Renegotiation Step 0/3] Re-negotiating with {peerConnection}..");

            // Step 1: CreateOffer
            var observer = CreateOfferObserver(completionCallback, peerConnection);
            try
            {
                peerConnection.CreateOffer(observer);
            }
            catch(Exception ex)
            {
                completionCallback.Error($"{nameof(peerConnection.CreateOffer)} failed: {ex.Message}");
                if(!(ex is ObjectDisposedException))
                {
                    _logger.Error(ex);
                }
            }
        }

        static Observer<RTCSessionDescription> CreateOfferObserver(Observer completionCallback, IPeerConnection peerConnection)
            => new Observer<RTCSessionDescription>()
                .OnError(completionCallback)
                .OnResult(offer =>
                {
                    // Step 2: Send the SDP, then SetLocalSessionDescription, 
                    // so the sdp processed by remote peer before they process ICE candidates,
                    // those generated from SetLocalSessionDescriptionAsync();
                    try
                    {
                        peerConnection.Device.EnqueueOffer(peerConnection.Id, peerConnection.LastOfferId, offer);
                        _logger.Debug($"[Renegotiation Step 1/3] Offer generated and sent for {peerConnection}.");
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(IRemoteDevice.EnqueueOffer)} failed: {ex.Message}");
                        if(!(ex is ObjectDisposedException))
                        {
                            _logger.Error(ex);
                        }
                        return;
                    }

                    // then send candidates later so they processed after the SDP is processed.
                    var observer = SetLocalSessionDescriptionObserver(completionCallback, peerConnection);
                    try
                    {
                        peerConnection.SetLocalSessionDescription(offer, observer);
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(peerConnection.SetLocalSessionDescription)} failed: {ex.Message}");
                        if(!(ex is ObjectDisposedException))
                        {
                            _logger.Error(ex);
                        }
                    }
                });

        static Observer SetLocalSessionDescriptionObserver(Observer completionCallback, IPeerConnection peerConnection)
            => new Observer()
                .OnError(completionCallback)
                .OnSuccess(delegate
                {
                    _logger.Debug($"[Renegotiation Step 2/3] Local offer set for {peerConnection}.");
                    completionCallback.Success();
                });
    }
}
