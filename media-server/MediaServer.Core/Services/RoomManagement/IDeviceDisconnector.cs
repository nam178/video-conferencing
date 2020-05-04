using MediaServer.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManagement
{
    public interface IDeviceDisconnector
    {
        Task DisconnectDeviceAsync(IRemoteDevice remoteDevice);
    }
}
