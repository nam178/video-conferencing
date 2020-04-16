using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services;
using MediaServer.Models;
using MediaServer.Rtc.Models;
using MediaServer.Rtc.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Rtc.Services
{
    sealed class PeerConnectionRequestHandler : IRemoteDeviceService<PeerConnectionRequest>
    {
        readonly IPeerConnectionRepository _peerConnectionRepository;
        readonly IPeerConnectionFactory _peerConnectionFactory;
        readonly IRemoteDeviceDataRepository _remoteDeviceDataRepository;
        readonly IDispatchQueue _centralDispatchQueue;

        public PeerConnectionRequestHandler(
            IPeerConnectionRepository peerConnectionRepository,
            IPeerConnectionFactory peerConnectionFactory,
            IRemoteDeviceDataRepository remoteDeviceDataRepository,
            IDispatchQueue centralDispatchQueue)
        {
            _peerConnectionRepository = peerConnectionRepository
                ?? throw new ArgumentNullException(nameof(peerConnectionRepository));
            _peerConnectionFactory = peerConnectionFactory
                ?? throw new ArgumentNullException(nameof(peerConnectionFactory));
            _remoteDeviceDataRepository = remoteDeviceDataRepository
                ?? throw new ArgumentNullException(nameof(remoteDeviceDataRepository));
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, PeerConnectionRequest request)
        {
            Require.NotEmpty(request.OfferedSessionDescription);

            // First, try to get existing peer connection for this device
            // TODO support multi PC per peer
            var pc = await _centralDispatchQueue.ExecuteAsync(delegate
            {
                return _peerConnectionRepository.Find(remoteDevice)?.FirstOrDefault();
            });

            // If no PeerConnection for this device, create one
            if(null == pc)
            {
                pc = _peerConnectionFactory.Create();
                // Jump back to the main queue to register the newly created PeerConnection
                await _centralDispatchQueue.ExecuteAsync(delegate
                {
                    var data = _remoteDeviceDataRepository.GetForDevice(remoteDevice);
                    if(data?.User == null)
                        throw new UnauthorizedAccessException();
                    _peerConnectionRepository.Add(data.User, remoteDevice, pc);
                });
            }

            // Update SDP 
            pc.RemoteSessionDescription = request.OfferedSessionDescription;
        }
    }
}
