using MediaServer.Common.Mediator;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Net
{
    /// <summary>
    /// This handles the event where an HTTP client accepted and ugpraded to web socket
    /// </summary>
    sealed class RemoteDeviceConnectedHandler : IHandler<RemoteDeviceWebSocketBased>
    {
        readonly IHandler<RemoteDeviceWebSocketBased, string> _commandHandler;

        // should be large enough to read network stream fast enough
        const int BUFFER_SIZE = 8 * 1024;

        public RemoteDeviceConnectedHandler(IHandler<RemoteDeviceWebSocketBased, string> commandHandler)
        {
            _commandHandler = commandHandler
                ?? throw new ArgumentNullException(nameof(commandHandler));
        }

        public async Task HandleAsync(RemoteDeviceWebSocketBased device)
        {
            var buff = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
            var messageBuilder = new StringBuilder();

            // Keep reading messages forever
            while(true)
            {
                // Read chunks of a message
                while(true)
                {
                    var tmp = await device.WebSocketClient.WebSocketContext.WebSocket.ReceiveAsync(buff, CancellationToken.None);

                    if(tmp.MessageType != System.Net.WebSockets.WebSocketMessageType.Text)
                        throw new NotSupportedException();
                    if(tmp.Count > 0 && tmp.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                        messageBuilder.Append(Encoding.UTF8.GetString(buff.Array, 0, tmp.Count));
                    if(tmp.EndOfMessage)
                    {
                        break;
                    }
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
