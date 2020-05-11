using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation
{
    sealed class AnswerMessageSubscriber : INegotiationMessageSubscriber
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public bool CanHandle(NegotiationMessage message)
        {
            return (message is SessionDescriptionMessage)
                && "answer".Equals(
                    ((SessionDescriptionMessage)message).SessionDescription.Type,
                    StringComparison.InvariantCultureIgnoreCase);
        }

        public void Handle(NegotiationMessage message, Observer completionCallback)
        {
            var sdp = ((SessionDescriptionMessage)message).SessionDescription;
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
