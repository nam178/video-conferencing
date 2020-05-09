using MediaServer.Core.Common;
using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface ITransceiverMetadataSetter
    {
        Task HandleAsync(IRemoteDevice device, TransceiverMetadata[] transceivers);
    }
}
