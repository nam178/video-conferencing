using MediaServer.Common.Mediator;
using NLog;
using System;
using System.Net;

namespace MediaServer.WebSocket.Net
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
            catch(System.Net.WebSockets.WebSocketException ex)
            {
                _logger.Warn($"WebSocket Error Device={remoteDevice}, Err{ex.Message}");
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                // always, upon the exit of this block
                // must trigger the DeviceDisconenctedHandler
                if(remoteDevice != null)
                {
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
    }
}
