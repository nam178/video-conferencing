using MediaServer.Models;

namespace MediaServer.Core.Models
{
    interface IRemoteDeviceDataWritable
    {
        Room Room { set; }

        UserProfile  User { set; }
    }
}
