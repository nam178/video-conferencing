using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class LocalTransceiverMetadataAckMessageSubscriber : IMessageSubscriber
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is LocalTransceiverMetadataAckMessage;

        public void Handle(Message message, Callback completionCallback)
        {
            var m = (LocalTransceiverMetadataAckMessage)message;
            try
            {
                m.PeerConnection.Room.VideoRouter.AckTransceiverMetadata(
                    m.PeerConnection,
                    m.Acked.TransceiverMid);

                completionCallback.Success();
            }
            catch(Exception ex)
            {
                completionCallback.Error(ex.Message);
                if(ex is ObjectDisposedException)
                {
                    _logger.Warn($"Failed handling transceiver metadata ack: {ex.Message}");
                    return;
                }
                throw;
            }
        }
    }
}
