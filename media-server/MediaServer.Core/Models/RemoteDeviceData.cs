using MediaServer.Models;

namespace MediaServer.Core.Models
{
    sealed class RemoteDeviceData : IRemoteDeviceData, IRemoteDeviceDataWritable
    {
        public Room Room { get; set; }

        public UserProfile User { get; set; }
    }
}
