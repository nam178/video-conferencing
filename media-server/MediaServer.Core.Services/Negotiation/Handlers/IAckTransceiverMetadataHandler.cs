using MediaServer.Common.Media;
using MediaServer.Core.Models;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    public interface IAckTransceiverMetadataHandler
    {
        void Handle(IRemoteDevice remoteDevice, TransceiverMetadata metadata);
    }
}