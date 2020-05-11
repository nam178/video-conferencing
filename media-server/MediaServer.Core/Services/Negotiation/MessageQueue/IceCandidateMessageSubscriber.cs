using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class IceCandidateMessageSubscriber : IMessageSubscriber
    {
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => (message is IceCandidateMessage);

        public void Handle(Message message, Observer completionCallback)
        {
            var iceCandidateMessage = ((IceCandidateMessage)message);

            // Remote ICE candidate? Receive it.
            if(iceCandidateMessage.IsRemote)
            {
                try
                {
                    message.PeerConnection.AddIceCandidate(iceCandidateMessage.Candidate);
                }
                catch(Exception ex)
                {
                    completionCallback.Error(
                        $"{nameof(message.PeerConnection.AddIceCandidate)} failed: {ex.Message}");
                    if(!(ex is ObjectDisposedException))
                    {
                        _logger.Error(ex);
                    }
                    return;
                }
            }
            // Locally generated ICE candidate? Send it.
            else
            {
                try
                {
                    message.PeerConnection.Device.EnqueueIceCandidate(
                        message.PeerConnection.Id, iceCandidateMessage.Candidate);
                }
                catch(Exception ex)
                {
                    completionCallback.Error(
                        $"{nameof(message.PeerConnection.AddIceCandidate)} failed: {ex.Message}");
                    if(!(ex is ObjectDisposedException))
                    {
                        _logger.Error(ex);
                    }
                    return;
                }
            }
            completionCallback.Success();
        }
    }
}
