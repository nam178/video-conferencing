using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services;
using MediaServer.Models;
using MediaServer.Rtc.Models;
using MediaServer.Rtc.Repositories;
using NLog;
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
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

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
            Require.NotEmpty(request.PeerConnectionId);

            UserProfile currentUser = default;
            IPeerConnection peerConnection = default;

            // Jump to the central queue and find if we have a PeerConnection for
            // the current user, with the specified PeerConnection id
            await _centralDispatchQueue.ExecuteAsync(delegate
            {
                var remoteDeviceData = _remoteDeviceDataRepository.GetForDevice(remoteDevice);
                if(remoteDeviceData?.User is null)
                    throw new UnauthorizedAccessException($"Device {remoteDevice} has not signed in");
                currentUser = remoteDeviceData.User;
                peerConnection = _peerConnectionRepository
                    .Find(remoteDeviceData.User)
                    .FirstOrDefault(peer => peer.Id == request.PeerConnectionId);
            });

            // If we don't yet have a PeerConnection with the id above,
            // create one. 
            // Note that don't do this in the main thread because creating one is slow.
            if(peerConnection is null)
            {
                peerConnection = _peerConnectionFactory.Create(request.PeerConnectionId);
                // Don't forget to register it
                try
                {
                    await _centralDispatchQueue.ExecuteAsync(() => _peerConnectionRepository.Add(currentUser, peerConnection));
                }
                catch(Exception)
                {
                    // On errors, ensure we dispose the peerConnection
                    peerConnection.Dispose();
                    throw;
                }
                _logger.Info($"New PeerConnection created {request.PeerConnectionId}");
            }

            peerConnection.RemoteSessionDescription = request.OfferedSessionDescription;
        }
    }
}
