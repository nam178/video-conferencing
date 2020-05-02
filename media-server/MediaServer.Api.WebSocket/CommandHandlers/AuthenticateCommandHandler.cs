﻿using MediaServer.Common.Mediator;
using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class AuthenticateCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.Authenticate>
    {
        public Task HandleAsync(IWebSocketRemoteDevice remoteDevice, Authenticate args)
        {
            // TODO implement actual login
            return remoteDevice.SendAsync("AuthenticationSuccess", new
            {
                // As the device ID is generated by the server,
                // send back the id so client knows its id
                DeviceId = remoteDevice.Id
            });
        }
    }
}