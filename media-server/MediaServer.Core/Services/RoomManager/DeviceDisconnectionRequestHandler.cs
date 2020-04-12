using MediaServer.Core.Common;
using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class DeviceDisconnectionRequestHandler : IRemoteDeviceRequestHandler<DeviceDisconnectionRequest>
    {
        public Task HandleAsync(IRemoteDevice arg1, DeviceDisconnectionRequest arg2)
        {
            throw new NotImplementedException();
        }
    }
}
