using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services.PeerConnection;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Core
{
    public class RTCSessionDescriptionHandlerTests
    {
        readonly Mock<IPeerConnectionRepository> _peerConnectionRepository = new Mock<IPeerConnectionRepository>();
        readonly Mock<IPeerConnectionFactory> _peerConnectionFactory = new Mock<IPeerConnectionFactory>();
        readonly Mock<IRemoteDeviceDataRepository> _remoteDeviceDataRepository = new Mock<IRemoteDeviceDataRepository>();
        readonly Mock<IRemoteDevice> _mockRemoteDevice = new Mock<IRemoteDevice>();
        readonly ThreadPoolDispatchQueue _centralDispatchQueue = new ThreadPoolDispatchQueue();
        readonly RTCSessionDescriptionHandler _handler;
        readonly Mock<IRoom> _mockRoom = new Mock<IRoom>();

        public RTCSessionDescriptionHandlerTests()
        {
            _centralDispatchQueue.Start();
            _mockRoom.Setup(x => x.PeerConnectionFactory).Returns(_peerConnectionFactory.Object);
            _handler = new RTCSessionDescriptionHandler(
                _peerConnectionRepository.Object,
                _remoteDeviceDataRepository.Object,
                _centralDispatchQueue
                );
        }

        [Theory]
        [MemberData(nameof(MockNotSignedInRemoteDeviceData))]
        public void HandleAsync_NotSignedIn_ThrowsUnauthorizedAccessException(IRemoteDeviceData remoteDeviceData)
        {
            _remoteDeviceDataRepository
                .Setup(x => x.GetForDevice(_mockRemoteDevice.Object))
                .Returns(remoteDeviceData);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async delegate
            {
                await _handler.HandleAsync(_mockRemoteDevice.Object, new RTCSessionDescription());
            });
        }

        public static IEnumerable<object[]> MockNotSignedInRemoteDeviceData()
        {
            yield return new[] { (IRemoteDeviceData)null };
            yield return new[] { new RemoteDeviceData { User = null } };
        }

        [Fact]
        public async Task HandleAsync_NoPeerConnectionYet_NewOneWillBeCreated()
        {
            var mockUser = new UserProfile(_mockRoom.Object);
            var mockPeerConnection = new Mock<IPeerConnection>();
            _remoteDeviceDataRepository
                .Setup(x => x.GetForDevice(_mockRemoteDevice.Object))
                .Returns(new RemoteDeviceData { User = mockUser });
            _peerConnectionRepository
                .Setup(x => x.Find(_mockRemoteDevice.Object))
                .Returns(new List<IPeerConnection>());
            _peerConnectionFactory
                .Setup(x => x.Create())
                .Returns(mockPeerConnection.Object);

            var request = new RTCSessionDescription();
            await _handler.HandleAsync(_mockRemoteDevice.Object, request);

            _peerConnectionRepository
                .Verify(x => x.Add(It.IsAny<UserProfile>(), It.IsAny<IRemoteDevice>(), It.IsAny<IPeerConnection>()), Times.Once);
            mockPeerConnection
                .Verify(x => x.SetRemoteSessionDescriptionAsync(It.IsAny<RTCSessionDescription>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_PeerConnectionCreatedTwice_LaterOneIsRejected()
        {
            var mockUser = new UserProfile(_mockRoom.Object);
            var currentPeerConnections = new List<IPeerConnection>();
            var peerConnection1 = new Mock<IPeerConnection>();

            _remoteDeviceDataRepository
                .Setup(x => x.GetForDevice(_mockRemoteDevice.Object))
                .Returns(new RemoteDeviceData { User = mockUser });

            _peerConnectionRepository
                .Setup(x => x.Find(_mockRemoteDevice.Object))
                .Returns(currentPeerConnections);

            // Simulate that when peerConnection1 is created,
            // someone else adds another peer connection.
            _peerConnectionFactory
                .Setup(x => x.Create())
                .Returns(peerConnection1.Object)
                .Callback(delegate
                {
                    currentPeerConnections.Add(Mock.Of<IPeerConnection>());
                });

            await Assert.ThrowsAsync<OperationCanceledException>(async delegate
            {
                await _handler.HandleAsync(_mockRemoteDevice.Object, new RTCSessionDescription());
            });
            _peerConnectionRepository
                .Verify(x => x.Add(It.IsAny<UserProfile>(), It.IsAny<IRemoteDevice>(), It.IsAny<IPeerConnection>()), Times.Never);
            peerConnection1
                .Verify(x => x.Dispose(), Times.Once);
        }
    }
}
