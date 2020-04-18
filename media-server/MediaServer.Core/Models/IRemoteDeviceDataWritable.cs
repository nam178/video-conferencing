using MediaServer.Models;

namespace MediaServer.Core.Models
{
    public interface IRemoteDeviceDataWritable
    {
        IRoom Room { set; }

        UserProfile  User { set; }
    }
}
