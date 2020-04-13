using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.Net
{
    sealed class WebSocketClient : IDisposable
    {
        readonly HttpListenerContext _httpContext;
        readonly string _name;
        readonly BlockingCollection<(string message, TaskCompletionSource<bool> Callback)> _messageQueue;

        internal HttpListenerWebSocketContext WebSocketContext { get; }

        public WebSocketClient(HttpListenerContext httpContext, HttpListenerWebSocketContext webSocketContext)
        {
            _httpContext = httpContext
                ?? throw new ArgumentNullException(nameof(httpContext));
            _name = $"{_httpContext.Request.RemoteEndPoint.Address}:{_httpContext.Request.RemoteEndPoint.Port}";
            _messageQueue = new BlockingCollection<(string message, TaskCompletionSource<bool> Callback)>();
            WebSocketContext = webSocketContext
                ?? throw new ArgumentNullException(nameof(webSocketContext));

            StartMessageQueue();
        }

        // The .NET framework implementation only allow us
        // to send one message at a time, so we're implement one queue per socket here.
        void StartMessageQueue()
        {
            Task.Run(async delegate
            {
                using(_messageQueue)
                {
                    foreach(var item in _messageQueue.GetConsumingEnumerable())
                    {
                        try
                        {
                            await WebSocketContext.WebSocket.SendAsync(
                                new System.ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(item.message)),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                            item.Callback.SetResult(true);
                        }
                        catch(Exception ex)
                        {
                            item.Callback.SetException(ex);
                        }
                    }
                }
            });
        }

        public Task SendAsync(string message)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            _messageQueue.Add((message, taskCompletionSource));
            return taskCompletionSource.Task;
        }

        public override string ToString() => $"[WebSocketClient {_name}]";

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

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
