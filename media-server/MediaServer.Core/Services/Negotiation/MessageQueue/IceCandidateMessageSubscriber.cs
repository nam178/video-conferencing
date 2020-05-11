using MediaServer.Common.Patterns;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class IceCandidateMessageSubscriber : IMessageSubscriber
    {
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
                    return;
                }
            }
            completionCallback.Success();
        }
    }
}
