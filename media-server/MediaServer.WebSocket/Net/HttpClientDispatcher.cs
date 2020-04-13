using MediaServer.Common.Mediator;
using NLog;
using System;
using System.Net;

namespace MediaServer.WebSocket.Net
{
    sealed class HttpClientDispatcher : IDispatcher<HttpListenerContext>
    {
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        readonly IHandler<IRemoteDeviceInternal> _remoteDeviceConnectedHandler;
        readonly IHandler<IRemoteDeviceInternal> _remoteDeviceDisconenctedHandler;

        public HttpClientDispatcher(
            IHandler<IRemoteDeviceInternal> remoteDeviceConnectedHandler,
            IHandler<IRemoteDeviceInternal> remoteDeviceDisconenctedHandler)
        {
            _remoteDeviceConnectedHandler = remoteDeviceConnectedHandler 
                ?? throw new ArgumentNullException(nameof(remoteDeviceConnectedHandler));
            _remoteDeviceDisconenctedHandler = remoteDeviceDisconenctedHandler 
                ?? throw new ArgumentNullException(nameof(remoteDeviceDisconenctedHandler));
        }

        public async void Dispatch(HttpListenerContext httpListenerContext)
        {
            RemoteDeviceWebSocketBased remoteDevice = default;
            try
            {
                _logger.Trace($"{httpListenerContext.Request.RemoteEndPoint.Address}:{httpListenerContext.Request.RemoteEndPoint.Port} connected.");

                using(httpListenerContext.Response)
                {
                    var webSocketContext = await httpListenerContext.AcceptWebSocketAsync(null);
                    remoteDevice = new RemoteDeviceWebSocketBased(new WebSocketClient(httpListenerContext, webSocketContext));
                    _logger.Trace($"{remoteDevice} Http -> WebSocket upgraded OK.");

                    using(webSocketContext.WebSocket)
                    {
                        await _remoteDeviceConnectedHandler.HandleAsync(remoteDevice);
                    }
                }
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
