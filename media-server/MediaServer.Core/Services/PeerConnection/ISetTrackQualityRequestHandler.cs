using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    public interface ISetTrackQualityRequestHandler
    {
        Task HandleAsync(IRemoteDevice device, SetTrackQualityRequest args);
    }
}
