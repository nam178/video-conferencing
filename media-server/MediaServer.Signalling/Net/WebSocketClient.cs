using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Net
{
    sealed class WebSocketClient
    {
        readonly HttpListenerContext _httpContext;
        readonly HttpListenerWebSocketContext _context;

        public WebSocketClient(HttpListenerContext httpContext, HttpListenerWebSocketContext context)
        {
            _httpContext = httpContext
                ?? throw new System.ArgumentNullException(nameof(httpContext));
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        public Task SendAsync(string message)
        {
            return _context.WebSocket.SendAsync(
                new System.ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
                );
        }

        public override string ToString() => $"[WebSocketClient {_httpContext.Request.RemoteEndPoint.Address}:{_httpContext.Request.RemoteEndPoint.Port}]";
    }
}
