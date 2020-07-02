using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Patterns;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Services.Common;
using MediaServer.Core.Services.RoomManagement;
using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class JoinRoomCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.JoinRoom>
    {
        readonly IMapper<IRemoteDevice, JoinRoomRequest, GenericResponse> _coreHandler;

        public JoinRoomCommandHandler(IMapper<IRemoteDevice, JoinRoomRequest, GenericResponse> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new ArgumentNullException(nameof(coreHandler));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, JoinRoom args)
        {
            Require.NotNull(args.RoomId);
            Require.NotNull(args.Username);

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
    }
}
