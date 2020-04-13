using MediaServer.Common.Mediator;
using MediaServer.Core.Services;
using MediaServer.Core.Services.ServerManager;
using MediaServer.WebSocket.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class CreateRoomCommandHandler : IHandler<IRemoteDeviceInternal, CommandArgs.CreateRoom>
    {
        readonly IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse> _coreHandler;

        public CreateRoomCommandHandler(IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new System.ArgumentNullException(nameof(coreHandler));
        }                                                                                                                                                        

        public async Task HandleAsync(IRemoteDeviceInternal remoteDevice, CommandArgs.CreateRoom args)
        {
            try
            {
                var result = await _coreHandler.HandleAsync(remoteDevice, new NewRoomRequest
                {
                    NewRoomName = args.NewRoomName
                });

                if(result.Success)
                    await remoteDevice.SendAsync("RoomCreated", result.CreatedRoomId);
                else
                    await remoteDevice.SendAsync("RoomCreationFailed", result.ErrorMessage);
            }
            catch(Exception ex)
            {
                await remoteDevice.SendAsync("RoomCreationFailed", ex.Message);
            }
        }
    }
}
