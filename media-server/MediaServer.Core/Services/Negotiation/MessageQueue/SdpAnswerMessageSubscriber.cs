using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class SdpAnswerMessageSubscriber : IMessageSubscriber
    {
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is SdpAnswerMessage;

        public void Handle(Message message, Observer completionCallback)
        {
            var answerMsg = ((SdpAnswerMessage)message);

            // Reject the answer if it's no longer valid
            if(answerMsg.OfferId != message.PeerConnection.LastOfferId)
            {
                completionCallback.Error($"Offer {answerMsg.OfferId} expired, rejecting.");
                return;
            }

            var observer = SetRemoteSessionDescriptionObserver(message, completionCallback, answerMsg);
            try
            {
                message.PeerConnection.SetRemoteSessionDescription(answerMsg.SessionDescription, observer);
            }
            catch(Exception ex)
            {
                completionCallback.Error($"{nameof(message.PeerConnection.SetRemoteSessionDescription)} failed: {ex.Message}");
                if(!(ex is ObjectDisposedException))
                {
                    _logger.Error(ex);
                }
            }
        }

        static Observer SetRemoteSessionDescriptionObserver(Message message, Observer completionCallback, SdpAnswerMessage answerMsg)
            => new Observer()
                .OnError(completionCallback)
                .OnSuccess(delegate
                {
                    _logger.Info($"[Renegotiation Step 3/3] remote answer {answerMsg} set for {message.PeerConnection}");

                    // When we receive an answer,
                    // that means the remote peer has acked that they 
                    // received the latest transceiver metadata update.
                    try
                    {
                        message.PeerConnection.Room.VideoRouter.ClearFrozenTransceivers(
                            message.PeerConnection.Device.Id,
                            message.PeerConnection.Id);
                    }
                    catch(Exception ex)
                    {
                        completionCallback.Error($"{nameof(message.PeerConnection.Room.VideoRouter.ClearFrozenTransceivers)} failed: {ex.Message}");
                        if(!(ex is ObjectDisposedException))
                        {
                            _logger.Error(ex);
                        }
                        return;
                    }
                    completionCallback.Success();
                });
    }
}
