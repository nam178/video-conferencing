using MediaServer.Core.Models;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManagement
{
    public interface ISyncMessenger
    {
        Task SendAsync(IRoom room);
    }
}
