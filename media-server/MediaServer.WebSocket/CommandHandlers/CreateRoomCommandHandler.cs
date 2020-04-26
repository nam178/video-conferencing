using MediaServer.Common.Mediator;
using MediaServer.Core.Models;
using MediaServer.Core.Services;
using MediaServer.Core.Services.ServerManager;
using MediaServer.WebSocket.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class CreateRoomCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.CreateRoom>
    {
        readonly IRemoteDeviceService<NewRoomRequest, RoomId> _coreHandler;

        public CreateRoomCommandHandler(IRemoteDeviceService<NewRoomRequest, RoomId> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new System.ArgumentNullException(nameof(coreHandler));
        }                                                                                                                                                        

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, CommandArgs.CreateRoom args)
        {
            var result = await _coreHandler.HandleAsync(remoteDevice, new NewRoomRequest
            {
                NewRoomName = args.NewRoomName
            });
            await remoteDevice.SendAsync("RoomCreated", result.ToString());
        }
    }
}
