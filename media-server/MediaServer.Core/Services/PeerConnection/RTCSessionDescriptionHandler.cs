using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class RTCSessionDescriptionHandler : IRemoteDeviceService<RTCSessionDescription>
    {
        readonly IPeerConnectionRepository _peerConnectionRepository;
        readonly IRemoteDeviceDataRepository _remoteDeviceDataRepository;
        readonly IDispatchQueue _centralDispatchQueue;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public RTCSessionDescriptionHandler(
            IPeerConnectionRepository peerConnectionRepository,
            IRemoteDeviceDataRepository remoteDeviceDataRepository,
            IDispatchQueue centralDispatchQueue)
        {
            _peerConnectionRepository = peerConnectionRepository
                ?? throw new ArgumentNullException(nameof(peerConnectionRepository));
            _remoteDeviceDataRepository = remoteDeviceDataRepository
                ?? throw new ArgumentNullException(nameof(remoteDeviceDataRepository));
            _centralDispatchQueue = centralDispatchQueue
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, RTCSessionDescription request)
        {
            Require.NotNull(request.Sdp);
            Require.NotNull(request.Type);

            // Get user and current IPeerConnection for this device
            var user = (User)null;
            var pc = (IPeerConnection)null;
            await _centralDispatchQueue.ExecuteAsync(delegate
            {
                var data = _remoteDeviceDataRepository.GetForDevice(remoteDevice);
                if(data?.User == null)
                    throw new UnauthorizedAccessException();
                user = data.User;
                pc = _peerConnectionRepository.Find(remoteDevice)?.FirstOrDefault();
            });

            // If no PeerConnection for this device, create one
            if(null == pc)
            {
                // Create PeerConnection outside of the main thread, because it's slow.
                pc = user.Room.PeerConnectionFactory.Create();
                _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

                // Jump back to the main thread to register it.
                await _centralDispatchQueue.ExecuteAsync(delegate
                {
                    // Double-check:
                    // We don't allow multiple PeerConnection per device yet
                    var existingPeerConnections = _peerConnectionRepository.Find(remoteDevice);
                    if(existingPeerConnections != null && existingPeerConnections.Any())
                    {
                        pc.Dispose();
                        _logger.Warn($"PeerConnection closed due to duplicate, user {user}, device {remoteDevice}");
                        throw new OperationCanceledException();
                    }
                    _peerConnectionRepository.Add(user, remoteDevice, pc);

                    // This is the first time is PeerConnection is created,
                    // we'll add ICE candidate observer
                    pc.ObserveIceCandidate(ice => remoteDevice
                        .SendIceCandidateAsync(ice)
                        .Forget($"Error when sending ICE candidate {ice} to device {remoteDevice}"));
                });
            }

            // Update SDP 
            await pc.SetRemoteSessionDescriptionAsync(request);
            _logger.Info($"Remote {request} SDP set for {pc}");
        }
    }
}
