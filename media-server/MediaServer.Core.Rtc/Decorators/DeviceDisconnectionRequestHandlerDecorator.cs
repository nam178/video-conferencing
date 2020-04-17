using MediaServer.Common.Threading;
using MediaServer.Core.Rtc.Repositories;
using MediaServer.Core.Services;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Models;
using NLog;
using System.Threading.Tasks;

namespace MediaServer.Core.Rtc.Decorators
{
    /// <summary>
    /// This decorator is to ensure we clean up our shit when a device disconnect
    /// </summary>
    sealed class DeviceDisconnectionRequestHandlerDecorator : IRemoteDeviceService<DeviceDisconnectionRequest>
    {
        readonly IRemoteDeviceService<DeviceDisconnectionRequest> _original;
        readonly IPeerConnectionRepository _peerConnectionRepository;
        readonly IDispatchQueue _dispatchQueue;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public DeviceDisconnectionRequestHandlerDecorator(
            IRemoteDeviceService<DeviceDisconnectionRequest> original,
            IPeerConnectionRepository peerConnectionRepository,
            IDispatchQueue dispatchQueue)
        {
            _original = original
                ?? throw new System.ArgumentNullException(nameof(original));
            _peerConnectionRepository = peerConnectionRepository
                ?? throw new System.ArgumentNullException(nameof(peerConnectionRepository));
            _dispatchQueue = dispatchQueue
                ?? throw new System.ArgumentNullException(nameof(dispatchQueue));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, DeviceDisconnectionRequest arg2)
        {
            try
            {
                await _original.HandleAsync(remoteDevice, arg2);
            }
            finally
            {
                await _dispatchQueue.ExecuteAsync(delegate
                {
                    var peerConnections = _peerConnectionRepository.Find(remoteDevice);
                    if(peerConnections != null)
                    {
                        foreach(var peer in peerConnections)
                        {
                            _logger.Debug($"Terminating {peer} due to device {remoteDevice} disconnect");
                            using(peer)
                            {
                                _peerConnectionRepository.Remove(peer);
                            }
                        }
                    }
                });
            }
        }
    }
}
