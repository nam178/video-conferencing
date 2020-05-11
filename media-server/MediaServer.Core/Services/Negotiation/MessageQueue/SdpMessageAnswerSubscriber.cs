using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class SdpMessageAnswerSubscriber : IMessageSubscriber
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message)
        {
            return message is SdpMessage
                && "answer".Equals(
                    ((SdpMessage)message).SessionDescription.Type,
                    StringComparison.InvariantCultureIgnoreCase);
        }

        public void Handle(Message message, Observer completionCallback)
        {
            var sdp = ((SdpMessage)message).SessionDescription;
            var observer = new Observer()
                .OnError(completionCallback)
                .OnSuccess(delegate
                {
                    _logger.Info($"[Renegotiation Step 3/3] remote answer {sdp} set for {message.PeerConnection}");
                    completionCallback.Success();
                });
            try
            {
                message.PeerConnection.SetRemoteSessionDescription(sdp, observer);
            }
            catch(Exception ex)
            {
                completionCallback.Error($"{nameof(message.PeerConnection.SetRemoteSessionDescription)} failed: {ex.Message}");
            }
        }
    }
}
