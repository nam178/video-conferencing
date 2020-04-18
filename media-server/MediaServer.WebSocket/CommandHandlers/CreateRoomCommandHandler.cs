﻿using MediaServer.Common.Mediator;
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
        readonly ICoreService<NewRoomRequest, RoomId> _coreHandler;

        public CreateRoomCommandHandler(ICoreService<NewRoomRequest, RoomId> coreHandler)
        {
            _coreHandler = coreHandler
                ?? throw new System.ArgumentNullException(nameof(coreHandler));
        }                                                                                                                                                        

        public async Task HandleAsync(IWebSocketRemoteDevice remoteDevice, CommandArgs.CreateRoom args)
        {
            try
            {
                var result = await _coreHandler.HandleAsync(remoteDevice, new NewRoomRequest
                {
                    NewRoomName = args.NewRoomName
                });
                await remoteDevice.SendAsync("RoomCreated", result.ToString());
            }
            catch(Exception ex)
            {
                await remoteDevice.SendAsync("RoomCreationFailed", ex.Message);
            }
        }
    }
}
