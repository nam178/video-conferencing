using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using MediaServer.Rtc.Models;
using MediaServer.Rtc.Repositories;
using MediaServer.Rtc.Services;
using MediaServer.WebRtc.Managed;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Core
{
    public class PeerConnectionRequestHandlerTests
    {
        readonly Mock<IPeerConnectionRepository> _peerConnectionRepository = new Mock<IPeerConnectionRepository>();
        readonly Mock<IPeerConnectionFactory> _peerConnectionFactory = new Mock<IPeerConnectionFactory>();
        readonly Mock<IRemoteDeviceDataRepository> _remoteDeviceDataRepository = new Mock<IRemoteDeviceDataRepository>();
        readonly Mock<IRemoteDevice> _remoteDevice = new Mock<IRemoteDevice>();
        readonly ThreadPoolDispatchQueue _centralDispatchQueue = new ThreadPoolDispatchQueue();
        readonly PeerConnectionRequestHandler _handler;

        public PeerConnectionRequestHandlerTests()
        {
            _handler = new PeerConnectionRequestHandler(
                _peerConnectionRepository.Object,
                _peerConnectionFactory.Object,
                _remoteDeviceDataRepository.Object,
                _centralDispatchQueue
                );
        }

        [Theory]
        [MemberData(nameof(MockNotSignedInRemoteDeviceData))]
        public void HandleAsync_NotSignedIn_ThrowsUnauthorizedAccessException(IRemoteDeviceData remoteDeviceData)
        {
            _remoteDeviceDataRepository
                .Setup(x => x.GetForDevice(_remoteDevice.Object))
                .Returns(remoteDeviceData);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async delegate
            {
                await _handler.HandleAsync(_remoteDevice.Object, new PeerConnectionRequest
                {
                    OfferedSessionDescription = new RTCSessionDescription()
                });
            });
        }

        public static IEnumerable<object[]> MockNotSignedInRemoteDeviceData()
        {
            yield return new[] { (IRemoteDeviceData)null };
            yield return new[] { new RemoteDeviceData { User = null } };
        }
    }
}
