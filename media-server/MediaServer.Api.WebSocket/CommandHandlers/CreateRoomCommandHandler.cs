using MediaServer.Core.Models;
using MediaServer.Api.WebSocket.Net;
using System.Threading.Tasks;
using MediaServer.Core.Services.RoomManagement;
using MediaServer.Common.Patterns;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class CreateRoomCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.CreateRoom>
    {
        readonly IMapper<IRemoteDevice, NewRoomRequest, RoomId> _coreHandler;

        public CreateRoomCommandHandler(IMapper<IRemoteDevice, NewRoomRequest, RoomId> coreHandler)
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
