using MediaServer.Common.Commands;
using MediaServer.Core.Common;
using MediaServer.Core.Services.RoomManagement;
using MediaServer.Signalling.Net;
using System.Threading.Tasks;

namespace MediaServer.Signalling.CommandHandlers
{
    sealed class CreateRoomCommandHandler : ICommandHandler<WebSocketClientRemoteDevice, CommandArgs.CreateRoom>
    {
        readonly IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse> _handler;

        public CreateRoomCommandHandler(IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse> handler)
        {
            _handler = handler
                ?? throw new System.ArgumentNullException(nameof(handler));
        }                                                                                                                                                        

        public async Task HandleAsync(WebSocketClientRemoteDevice remoteDevice, CommandArgs.CreateRoom args)
        {
            var result = await _handler.HandleAsync(remoteDevice, new NewRoomRequest
            {
                NewRoomName = args.NewRoomName
            });

            if(result.Success)
                await remoteDevice.SendAsync("RoomCreated", result.CreatedRoomId);
            else
                await remoteDevice.SendAsync("RoomCreationFailed", args.NewRoomName);
        }
    }
}
