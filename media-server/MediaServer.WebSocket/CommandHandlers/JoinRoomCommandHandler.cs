using MediaServer.Common.Mediator;
using MediaServer.Core.Common;
using MediaServer.Core.Models;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManager;
using MediaServer.WebSocket.CommandArgs;
using MediaServer.WebSocket.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class JoinRoomCommandHandler : IHandler<IRemoteDeviceInternal, CommandArgs.JoinRoom>
    {
        readonly IRemoteDeviceRequestHandler<JoinRoomRequest, GenericResponse> _coreHandler;

        public JoinRoomCommandHandler(IRemoteDeviceRequestHandler<JoinRoomRequest, GenericResponse> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new ArgumentNullException(nameof(coreHandler));
        }

        public async Task HandleAsync(IRemoteDeviceInternal remoteDevice, JoinRoom args)
        {
            try
            {
                var result = await _coreHandler.HandleAsync(remoteDevice, new JoinRoomRequest
                {
                    Username = args.Username,
                    RoomId = RoomId.FromString(args.RoomId)
                });
                if(result.Success)
                    await remoteDevice.SendAsync("JoinRoomSuccess", null);
                else
                    await remoteDevice.SendAsync("JoinRoomFailed", result.ErrorMessage);
            }
            catch(Exception ex)
            {
                await remoteDevice.SendAsync("JoinRoomFailed", ex.Message);
            }
        }
    }
}
