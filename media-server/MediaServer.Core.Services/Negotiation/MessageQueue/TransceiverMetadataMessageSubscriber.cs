using MediaServer.Common.Patterns;
using NLog;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class TransceiverMetadataMessageSubscriber : IMessageSubscriber
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool CanHandle(Message message) => message is TransceiverMetadataMessage;

        public void Handle(Message message, Callback completionCallback)
        {
            try
            {
                var msg = ((TransceiverMetadataMessage)message);
                if(null == msg.Transceivers)
                {
                    throw new NullReferenceException(nameof(msg.Transceivers));
                }

                foreach(var transceiver in msg.Transceivers)
                {
                    if(string.IsNullOrWhiteSpace(transceiver.TransceiverMid))
                        throw new ArgumentException("TrackId is NULL or empty");

                    message.PeerConnection.Room.VideoRouter.SetRemoteTransceiverMetadata(transceiver);
                }

                completionCallback.Success();
            }
            catch(Exception ex)
            {
                completionCallback.Error(ex.Message);
                if(ex is ObjectDisposedException)
                {
                    _logger.Warn($"Error occured while setting remote transceiver metadata: {ex.Message}");
                }
                else throw;
            }
        }
    }
}
