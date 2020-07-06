using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class LocalTransceiverMetadataMessageSubscriber : IMessageSubscriber
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is LocalTransceiverMetadataMessage;

        public void Handle(Message message, Callback completionCallback)
        {
            var m = (LocalTransceiverMetadataMessage)message;
            try
            {
                message.PeerConnection.Device.EnqueueTransceiverMetadata(m.TransceiverMetadata);
                completionCallback.Success();
            }
            catch(Exception ex)
            {
                completionCallback.Error(ex.Message);
                if(ex is ObjectDisposedException)
                {
                    _logger.Warn($"Error occured while sending local transceiver metadata to client: {ex.Message}");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
