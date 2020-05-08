using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface ITransceiverInfoSetter
    {
        Task HandleAsync(IRemoteDevice device, TransceiverInfo[] transceivers);
    }
}
