using MediaServer.Core.Models;
using MediaServer.Models;

namespace MediaServer.Core.Repositories
{
    /// <summary>
    /// Used to look up what room/user profile a device is mapped to;
    /// Should be a singleton. Not thread safe. Use within the central dispatch queue.
    /// </summary>
    interface IRemoteDeviceUserProfileMappings
    {
        (IRemoteDevice Device, Room Room, UserProfile UserProfile) GetMappingForDevice(IRemoteDevice device);

        void SetMappingForDevice(IRemoteDevice device, Room room, UserProfile userProfile);

        void DeleteMappingForDevice(IRemoteDevice device);
    }
}
