using MediaServer.Common.Commands;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.WebSocket
{
    sealed class WebSocketServer : IHostedService
    {
        readonly HttpListener _httpListener;
        readonly ICommandHandler<WebSocketClient, string> _commandHandler;
        readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private TcpListener t;

#if DEBUG
        const int BUFFER_SIZE = 16;
#else
        const int BUFFER_SIZE = 8 * 1024;
#endif

        public WebSocketServer(ICommandHandler<WebSocketClient, string> commandHandler)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:8080/");
            _commandHandler = commandHandler 
                ?? throw new ArgumentNullException(nameof(commandHandler));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _httpListener.Start();
            _logger.Info("WebSocket server started");

            BeginAcceptingConnections();
            return Task.CompletedTask;
        }

        async void BeginAcceptingConnections()
        {
            while(true)
            {
                var context = await  _httpListener.GetContextAsync();
                BeginHandling(context);
            }
        }

        async void BeginHandling(HttpListenerContext context)
        {
            try
            {
                using(context.Response)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    var buff = new ArraySegment<byte>(new byte[BUFFER_SIZE]);
                    var messageBuilder = new StringBuilder();
                    var client = new WebSocketClient(context, webSocketContext);

                    _logger.Info($"WebSocket client connected; {client}");

                    using(webSocketContext.WebSocket)
                    {
                        // Keep reading messages forever
                        while(true)
                        {
                            // Read chunks of a message
                            while(true)
                            {
                                var t = await webSocketContext.WebSocket.ReceiveAsync(buff, CancellationToken.None);

                                if(t.MessageType != System.Net.WebSockets.WebSocketMessageType.Text)
                                {
                                    throw new NotSupportedException();
                                }
                                if(t.Count > 0 && t.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                                {
                                    messageBuilder.Append(Encoding.UTF8.GetString(buff.Array, 0, t.Count));
                                }
                                if(t.EndOfMessage)
                                {
                                    break;
                                }
                            }

                            // At this point, fully got the message;
                            // Just pass it to the handler
                            try
                            {
                                await _commandHandler.HandleAsync(client, messageBuilder.ToString());
                            }
                            finally
                            {
                                messageBuilder.Clear();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _httpListener.Stop();
            return Task.CompletedTask;
        }
    }
}
