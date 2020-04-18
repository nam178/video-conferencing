using MediaServer.Common.Threading;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.PeerConnection
{
    sealed class RTCIceCandidateHandler : IRemoteDeviceService<RTCIceCandidate>
    {
        readonly IDispatchQueue _centralQueue;
        readonly IPeerConnectionRepository _peerConnectionRepository;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public RTCIceCandidateHandler(
            IDispatchQueue centralQueue,
            IPeerConnectionRepository peerConnectionRepository)
        {
            _centralQueue = centralQueue 
                ?? throw new System.ArgumentNullException(nameof(centralQueue));
            _peerConnectionRepository = peerConnectionRepository 
                ?? throw new System.ArgumentNullException(nameof(peerConnectionRepository));
        }

        public async Task HandleAsync(IRemoteDevice remoteDevice, RTCIceCandidate iceCandidate)
        {
            var peerConnection = await _centralQueue.ExecuteAsync(delegate
            {
                var peerConnections = _peerConnectionRepository.Find(remoteDevice);
                if(!peerConnections.Any())
                {
                    throw new InvalidOperationException(
                        $"Remote device has not made any PeerConnection, " +
                        $"rejecting ICE candidate."
                        );
                }
                // TODO: support multiple PeerConnections per device
                return peerConnections.First();
            });

            peerConnection.AddIceCandidate(iceCandidate);
            _logger.Info($"Ice candidate {iceCandidate} added to {peerConnection}");
        }
    }
}
