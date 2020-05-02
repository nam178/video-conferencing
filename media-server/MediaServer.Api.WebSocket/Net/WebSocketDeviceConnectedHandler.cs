using MediaServer.Api.WebSocket.Errors;
using MediaServer.Common.Mediator;
using MediaServer.Common.Utils;
using MediaServer.Models;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.Net
{
    /// <summary>
    /// This handles the event where an HTTP client accepted and ugpraded to web socket
    /// </summary>
    sealed class WebSocketDeviceConnectedHandler : IHandler<IWebSocketRemoteDevice>
    {
        readonly IHandler<IWebSocketRemoteDevice, string> _commandHandler;
        readonly IWatchDog _watchDog;

        // should be large enough to read network stream fast enough
        const int BUFFER_SIZE = 8 * 1024;

        public WebSocketDeviceConnectedHandler(
            IHandler<IWebSocketRemoteDevice, string> commandHandler, 
            IWatchDog watchDog)
        {
            _commandHandler = commandHandler
                ?? throw new ArgumentNullException(nameof(commandHandler));
            _watchDog = watchDog 
                ?? throw new ArgumentNullException(nameof(watchDog));
        }

        public async Task HandleAsync(IWebSocketRemoteDevice device)
        {
            var buff = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
            var messageBuilder = new StringBuilder();
            using var watcher = _watchDog.Watch(device);

            // Keep reading messages forever
            while(true)
            {
                // For each message received, we'll 
                // update the watchdog so it won't stall.
                watcher.Refresh();

                // Read chunks of a message
                while(true)
                {
                    WebSocketReceiveResult tmp;
                    try
                    {
                        tmp = await device.WebSocketClient.WebSocketContext.WebSocket.ReceiveAsync(buff, CancellationToken.None);
                    }
                    // When the WebSocketClient is disposed
                    // by some other thread, it will throw InvalidOperationException
                    catch(InvalidOperationException ex)
                    {
                        throw new WebSocketClientDisposedException(ex.Message);
                    }

                    if(tmp.MessageType == WebSocketMessageType.Close)
                        return;
                    if(tmp.MessageType != WebSocketMessageType.Text)
                        throw new NotSupportedException();
                    if(tmp.Count > 0 && tmp.MessageType == WebSocketMessageType.Text)
                        messageBuilder.Append(Encoding.UTF8.GetString(buff.Array, 0, tmp.Count));
                    if(tmp.EndOfMessage)
                        break;
                }

                // At this point, fully got the message;
                // Just pass it to the handler
                try
                {
                    await _commandHandler.HandleAsync(device, messageBuilder.ToString());
                }
                finally
                {
                    messageBuilder.Clear();
                }
            }
        }
    }
}
