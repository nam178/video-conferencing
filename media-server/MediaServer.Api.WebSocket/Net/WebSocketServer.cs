using MediaServer.Common.Patterns;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.Net
{
    sealed class WebSocketServer : IHostedService
    {
        readonly HttpListener _httpListener;
        readonly IDispatcher<HttpListenerContext> _httpClientDispatcher;
        readonly static ILogger _logger = LogManager.GetCurrentClassLogger();

        public WebSocketServer(IDispatcher<HttpListenerContext> httpClientDispatcher)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(Environment.GetEnvironmentVariable("HTTP_LISTENER_PREFIX"));
            _httpClientDispatcher = httpClientDispatcher
                ?? throw new ArgumentNullException(nameof(httpClientDispatcher));
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
                _httpClientDispatcher.Dispatch(await _httpListener.GetContextAsync());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _httpListener.Stop();
            return Task.CompletedTask;
        }
    }
}
