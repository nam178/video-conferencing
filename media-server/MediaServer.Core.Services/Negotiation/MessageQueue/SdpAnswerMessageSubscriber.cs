using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class SdpAnswerMessageSubscriber : IMessageSubscriber
    {
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is SdpAnswerMessage;

        public void Handle(Message message, Callback completionCallback)
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

        static Callback SetRemoteSessionDescriptionObserver(Message message, Callback completionCallback, SdpAnswerMessage answerMsg)
            => new Callback()
                .OnError(completionCallback)
                .OnSuccess(delegate
                {
                    _logger.Info($"[Renegotiation Step 3/3] remote answer {answerMsg} set for {message.PeerConnection}");
                    completionCallback.Success();
                });
    }
}
