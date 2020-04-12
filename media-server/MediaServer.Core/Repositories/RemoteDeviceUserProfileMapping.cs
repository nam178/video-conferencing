using MediaServer.Core.Models;
using MediaServer.Models;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    sealed class RemoteDeviceUserProfileMapping : IRemoteDeviceUserProfileMappings
    {
        readonly Dictionary<IRemoteDevice, (Room Room, UserProfile UserProfile)> _mappings = new Dictionary<IRemoteDevice, (Room, UserProfile)>();

        public void DeleteMappingForDevice(IRemoteDevice device)
        {
            if(_mappings.ContainsKey(device))
            {
                _mappings.Remove(device);
            }
        }

        public (IRemoteDevice Device, Room Room, UserProfile UserProfile) GetMappingForDevice(IRemoteDevice device)
        {
            if(_mappings.ContainsKey(device))
            {
                var tmp = _mappings[device];
                return (device, tmp.Room, tmp.UserProfile);
            }
            return (device, null, null);
        }

        public void SetMappingForDevice(IRemoteDevice device, Room room, UserProfile userProfile)
        {
            _mappings[device] = (room, userProfile);
        }
    }
}
