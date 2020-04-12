using MediaServer.Common.Mediator;
using MediaServer.Core.Common;
using MediaServer.Core.Services.ServerManager;
using MediaServer.Signalling.Net;
using System;
using System.Threading.Tasks;

namespace MediaServer.Signalling.CommandHandlers
{
    sealed class CreateRoomCommandHandler : IHandler<RemoteDeviceWebSocketBased, CommandArgs.CreateRoom>
    {
        readonly IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse> _coreHandler;

        public CreateRoomCommandHandler(IRemoteDeviceRequestHandler<NewRoomRequest, NewRoomResponse> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new System.ArgumentNullException(nameof(coreHandler));
        }                                                                                                                                                        

        public async Task HandleAsync(RemoteDeviceWebSocketBased remoteDevice, CommandArgs.CreateRoom args)
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
