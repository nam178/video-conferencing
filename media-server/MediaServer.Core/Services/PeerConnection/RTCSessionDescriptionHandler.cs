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
            var peerConnection = (IPeerConnection)null;
            await _centralDispatchQueue.ExecuteAsync(delegate
            {
                var data = _remoteDeviceDataRepository.GetForDevice(remoteDevice);
                if(data?.User == null)
                    throw new UnauthorizedAccessException();
                user = data.User;
                peerConnection = _peerConnectionRepository.Find(remoteDevice)?.FirstOrDefault();
            });

            // If no PeerConnection for this device, create one
            if(null == peerConnection)
            {
                peerConnection = await CreatePeerConnectionAsync(remoteDevice, user);
            }

            // Finally do the SDP exchange, 
            // it has to be done in this exact oder:
            await peerConnection.SetRemoteSessionDescriptionAsync(request);
            _logger.Info($"Remote {request} SDP set for {peerConnection}");

            // Create answer and send it.
            var answer = await peerConnection.CreateAnswerAsync();
            _logger.Info($"Answer {answer} created for {peerConnection}");
            await remoteDevice.SendSessionDescriptionAsync(answer);
            _logger.Info($"Answer sent for {peerConnection}");

            // SetLocalSessionDescriptionAsync() must be after SendSessionDescriptionAsync()
            // because it SetLocalSessionDescriptionAsync() generates ICE candidates,
            // and we want to send ICE candidates after remote SDP is set.
            await peerConnection.SetLocalSessionDescriptionAsync(answer);
            _logger.Info($"Local description {answer} set for {peerConnection}");
        }

        async Task<IPeerConnection> CreatePeerConnectionAsync(IRemoteDevice remoteDevice, User user)
        {
            // Create PeerConnection outside of the main thread, because it's slow.
            IPeerConnection peerConnection = user.Room.PeerConnectionFactory.Create(remoteDevice);
            _logger.Info($"PeerConnection created, user {user}, device {remoteDevice}");

            // Jump back to the main thread to register it.
            await _centralDispatchQueue.ExecuteAsync(delegate
            {
                // Double-check:
                // We don't allow multiple PeerConnection per device yet
                var existingPeerConnections = _peerConnectionRepository.Find(remoteDevice);
                if(existingPeerConnections != null && existingPeerConnections.Any())
                {
                    peerConnection.Dispose();
                    _logger.Warn($"PeerConnection closed due to duplicate, user {user}, device {remoteDevice}");
                    throw new OperationCanceledException();
                }
                _peerConnectionRepository.Add(user, remoteDevice, peerConnection);

                // This is the first time is PeerConnection is created,
                // we'll add ICE candidate observer
                peerConnection.ObserveIceCandidate(ice => remoteDevice
                    .SendIceCandidateAsync(ice)
                    .Forget($"Error when sending ICE candidate {ice} to device {remoteDevice}"));
            });
            return peerConnection;
        }
    }
}
