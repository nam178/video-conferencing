using MediaServer.Common.Mediator;
using MediaServer.Core.Common;
using MediaServer.Core.Models;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Signalling.CommandArgs;
using MediaServer.Signalling.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.Signalling.CommandHandlers
{
    sealed class JoinRoomCommandHandler : IHandler<RemoteDeviceWebSocketBased, CommandArgs.JoinRoom>
    {
        readonly IRemoteDeviceRequestHandler<JoinRoomRequest, GenericResponse> _coreHandler;

        public JoinRoomCommandHandler(IRemoteDeviceRequestHandler<JoinRoomRequest, GenericResponse> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new ArgumentNullException(nameof(coreHandler));
        }

        public async Task HandleAsync(RemoteDeviceWebSocketBased remoteDevice, JoinRoom args)
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
