using MediaServer.Api.WebSocket.Net;
using System;

namespace MediaServer.Api.WebSocket.Errors
{
    sealed class WebSocketClientDisposedException : ObjectDisposedException
    {
        public WebSocketClientDisposedException(string msg) : base(msg)
        {
        }
    }
}
