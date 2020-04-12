using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Net
{
    sealed class WebSocketClient
    {
        readonly HttpListenerContext _httpContext;
        readonly string _name;

        internal HttpListenerWebSocketContext WebSocketContext { get; }

        public WebSocketClient(HttpListenerContext httpContext, HttpListenerWebSocketContext webSocketContext)
        {
            _httpContext = httpContext
                ?? throw new System.ArgumentNullException(nameof(httpContext));
            _name = $"{_httpContext.Request.RemoteEndPoint.Address}:{_httpContext.Request.RemoteEndPoint.Port}";
            WebSocketContext = webSocketContext
                ?? throw new System.ArgumentNullException(nameof(webSocketContext));
        }

        public Task SendAsync(string message)
        {
            return WebSocketContext.WebSocket.SendAsync(
                new System.ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
                );
        }

        public override string ToString() => $"[WebSocketClient {_name}]";
    }
}
