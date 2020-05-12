using MediaServer.Common.Patterns;
using System;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class TransceiverMetadataMessageSubscriber : IMessageSubscriber
    {
        public bool CanHandle(Message message) => message is TransceiverMetadataMessage;

        public void Handle(Message message, Observer completionCallback)
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

                message.PeerConnection.Room.VideoRouter.SetRemoteTransceiverMetadata(
                    message.PeerConnection.Device.Id,
                    transceiver.TransceiverMid,
                    transceiver.TrackQuality,
                    transceiver.Kind);
            }
        }
    }
}
