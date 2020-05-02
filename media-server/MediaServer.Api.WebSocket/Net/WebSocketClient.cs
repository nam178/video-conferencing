using MediaServer.Api.WebSocket.Errors;
using MediaServer.Common.Threading;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.Net
{
    sealed class WebSocketClient : IDisposable
    {
        readonly HttpListenerContext _httpContext;
        readonly string _name;
        // Notes
        // Each WebSocketClient requires its own outbound message queue.
        // This to ensure one client won't block other clients when we need 
        // to send out notifications,
        // Also, the underlying WebSocket library doesn't support sending
        // multiple message at once.
        readonly ThreadPoolDispatchQueue _outboundMessageQueue = new ThreadPoolDispatchQueue();

        internal HttpListenerWebSocketContext WebSocketContext { get; }

        public WebSocketClient(HttpListenerContext httpContext, HttpListenerWebSocketContext webSocketContext)
        {
            _httpContext = httpContext
                ?? throw new ArgumentNullException(nameof(httpContext));
            _name = $"{_httpContext.Request.RemoteEndPoint.Address}:{_httpContext.Request.RemoteEndPoint.Port}";
            _outboundMessageQueue.Start();
            WebSocketContext = webSocketContext
                ?? throw new ArgumentNullException(nameof(webSocketContext));
        }

        public Task SendAsync(string message)
        {
            if(Interlocked.CompareExchange(ref _disposed, 0, 0) == 1)
                throw new WebSocketClientDisposedException($"message could not be sent because {this} is closed");
            return _outboundMessageQueue.ExecuteAsync(async delegate
            {
                await WebSocketContext.WebSocket.SendAsync(
                    new System.ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            });
        }

        public override string ToString() => $"[WebSocketClient {_name}]";

        int _disposed;

        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;
            try
            {
                _outboundMessageQueue.Dispose();
            }
            catch { }
            using(WebSocketContext.WebSocket)
            {
                try
                {
                    WebSocketContext.WebSocket
                        .CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Server deliberately close the connection", CancellationToken.None)
                        .Wait(TimeSpan.FromSeconds(5));
                }
                catch { }
            }
        }
    }
}
