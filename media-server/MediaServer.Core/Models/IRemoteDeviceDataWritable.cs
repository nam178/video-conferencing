using MediaServer.Models;

namespace MediaServer.Core.Models
{
    public interface IRemoteDeviceDataWritable
    {
        Room Room { set; }

        UserProfile  User { set; }
    }
}
