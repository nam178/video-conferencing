using MediaServer.Core.Models;
using MediaServer.Models;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    sealed class RemoteDeviceDataRepository : IRemoteDeviceDataRepository
    {
        readonly Dictionary<IRemoteDevice, IRemoteDeviceData> _mappings = new Dictionary<IRemoteDevice, IRemoteDeviceData>();

        public IRemoteDeviceData GetForDevice(IRemoteDevice device)
        {
            if(_mappings.ContainsKey(device))
            {
                return _mappings[device];
            }
            return null;
        }

        public void SetForDevice(IRemoteDevice device, Action<IRemoteDeviceDataWritable> setter)
        {
            var tmp = GetForDevice(device) ?? new RemoteDeviceData();
            setter((IRemoteDeviceDataWritable)tmp);
            _mappings[device] = tmp;
        }

        public void DeleteForDevice(IRemoteDevice device)
        {
            if(_mappings.ContainsKey(device))
            {
                _mappings.Remove(device);
            }
        }
    }
}
