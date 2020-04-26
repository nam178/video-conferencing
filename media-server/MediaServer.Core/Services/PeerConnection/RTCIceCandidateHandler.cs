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
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public Task HandleAsync(IRemoteDevice remoteDevice, RTCIceCandidate iceCandidate)
        {
            var peerConnection = remoteDevice.GetCustomData().PeerConnections.FirstOrDefault();
            if(null == peerConnection)
            {
                throw new InvalidOperationException($"Device {remoteDevice} has no PeerConnection");
            }
            peerConnection.AddIceCandidate(iceCandidate);
            _logger.Trace($"Ice candidate {iceCandidate} added to {peerConnection}");

            return Task.CompletedTask;
        }
    }
}
