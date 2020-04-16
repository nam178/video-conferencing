using MediaServer.Models;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.Net
{
    interface IWebSocketRemoteDevice : IRemoteDevice, IDisposable
    {
        /// <summary>
        /// Unique id of this device (within the scope of this server)
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Expose the underlying websocket for internal use
        /// </summary>
        WebSocketClient WebSocketClient { get; }

        /// <summary>
        /// Expose the SendAsync() method for internal use
        /// </summary>
        /// <param name="command">Name of the command to send</param>
        /// <param name="args">Arguments to that command</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">This may throw exception when the underlying web socket fails</exception>
        Task SendAsync(string command, object args);
    }
}