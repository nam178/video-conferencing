using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    sealed class NewPeerConnectionRequestHandler : IRemoteDeviceService<NewPeerConnectionRequest>
    {
        public Task HandleAsync(IRemoteDevice arg1, NewPeerConnectionRequest arg2)
        {
            throw new NotImplementedException();
        }
    }
}
