using MediaServer.Common.Patterns;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.Net
{
    sealed class HttpClientDispatcher : IDispatcher<HttpListenerContext>
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        readonly IHandler<IWebSocketRemoteDevice> _remoteDeviceConnectedHandler;
        readonly IHandler<IWebSocketRemoteDevice> _remoteDeviceDisconenctedHandler;

        public HttpClientDispatcher(
            IHandler<IWebSocketRemoteDevice> remoteDeviceConnectedHandler,
            IHandler<IWebSocketRemoteDevice> remoteDeviceDisconenctedHandler)
        {
            _remoteDeviceConnectedHandler = remoteDeviceConnectedHandler
                ?? throw new ArgumentNullException(nameof(remoteDeviceConnectedHandler));
            _remoteDeviceDisconenctedHandler = remoteDeviceDisconenctedHandler
                ?? throw new ArgumentNullException(nameof(remoteDeviceDisconenctedHandler));
        }

        public async void Dispatch(HttpListenerContext httpListenerContext)
        {
            WebSocketRemoteDevice remoteDevice = default;
            try
            {
                _logger.Trace($"{httpListenerContext.Request.RemoteEndPoint.Address}:{httpListenerContext.Request.RemoteEndPoint.Port} connected.");
                using(httpListenerContext.Response)
                {
                    var webSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);
                    remoteDevice = new WebSocketRemoteDevice(new WebSocketClient(httpListenerContext, webSocketContext));
                    _logger.Trace($"{remoteDevice} Http -> WebSocket upgraded OK.");
                    using(webSocketContext.WebSocket)
                    {
                        await _remoteDeviceConnectedHandler.HandleAsync(remoteDevice);
                    }
                }
            }
            catch(IOException ex)
            {
                _logger.Warn($"WebSocket Error Device={remoteDevice}, Err{ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                await CleanUpAsync(remoteDevice);
            }
        }

        async Task CleanUpAsync(WebSocketRemoteDevice remoteDevice)
        {
            if(remoteDevice == null)
                return;

            using(remoteDevice)
            {
                _logger.Warn($"Device {remoteDevice} disconnected.");
                await _remoteDeviceDisconenctedHandler
                    .HandleAsync(remoteDevice)
                    .ContinueWith(task =>
                    {
                        if(task.Exception != null)
                        {
                            _logger.Error("remoteDeviceDisconenctedHandler throws exception", task.Exception);
                        }
                    });
            }
        }
    }
}
