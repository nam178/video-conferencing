using MediaServer.Common.Patterns;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Common;
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
                    // SetLocalSessionDescription() first before calling GetLocalTransceiverMetadata(),
                    // otherwise we'll get NULL transceiver mids.
                    var observer = SetLocalSessionDescriptionObserver(completionCallback, peerConnection, offer);
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

        static Observer SetLocalSessionDescriptionObserver(Observer completionCallback, IPeerConnection peerConnection, RTCSessionDescription offer)
            => new Observer()
                .OnError(completionCallback)
                .OnSuccess(delegate
                {
                    // Generate transceiver metadata and send along with the offer.
                    try
                    {
                        var transceivers = peerConnection.Room.VideoRouter.GetLocalTransceiverMetadata(
                                peerConnection.Device.Id,
                                peerConnection.Id);
                        peerConnection.Device.EnqueueOffer(peerConnection.Id, peerConnection.LastOfferId, offer,
                            transceivers);
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

                    _logger.Debug($"[Renegotiation Step 2/3] Local offer set for {peerConnection}.");
                    completionCallback.Success();
                });
    }
}
