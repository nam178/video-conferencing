using MediaServer.Core.Models;
using MediaServer.Models;
using System;

namespace MediaServer.Core.Repositories
{
    /// <summary>
    /// Allows attaching/removing custom data into/from remote devices.
    /// </summary>
    /// <remarks>
    /// Not thread safe. 
    /// Must be used from the central dispathc queue.
    /// </remarks>
    interface IRemoteDeviceDataRepository
    {
        IRemoteDeviceData GetForDevice(IRemoteDevice device);

        void SetForDevice(IRemoteDevice device, Action<IRemoteDeviceDataWritable> setter);

        void DeleteForDevice(IRemoteDevice device);
    }
}
