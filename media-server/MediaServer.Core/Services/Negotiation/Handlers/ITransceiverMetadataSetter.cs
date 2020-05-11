using MediaServer.Core.Common;
using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.Negotiation.Handlers
{
    public interface ITransceiverMetadataSetter
    {
        Task HandleAsync(IRemoteDevice device, TransceiverMetadata[] transceivers);
    }
}
