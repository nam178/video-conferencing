using MediaServer.Models;

namespace MediaServer.Core.Models
{
    sealed class RemoteDeviceData : IRemoteDeviceData, IRemoteDeviceDataWritable
    {
        public IRoom Room { get; set; }

        public UserProfile User { get; set; }
    }
}
