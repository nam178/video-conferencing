using MediaServer.Common.Patterns;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class IceCandidateMessageSubscriber : IMessageSubscriber
    {
        public bool CanHandle(Message message) => message is IceCandidateMessage;

        public void Handle(Message message, Observer completionCallback)
        {
            try
            {
                message.PeerConnection.AddIceCandidate(((IceCandidateMessage)message).Candidate);
            }
            catch(Exception ex)
            {
                completionCallback.Error($"{nameof(message.PeerConnection.AddIceCandidate)} failed: {ex.Message}");
            }
            completionCallback.Success();
        }
    }
}
