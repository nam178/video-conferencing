using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Core.Services.ServerManager;
using MediaServer.Models;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Core
{
    public class NewRoomRequestHandlerTests
    {
        readonly ThreadPoolDispatchQueue _dispatchQueue = new ThreadPoolDispatchQueue();
        readonly Mock<IRoomRepository> _roomRepository = new Mock<IRoomRepository>();
        readonly Mock<IRoomFactory> _mockRoomFactory = new Mock<IRoomFactory>();
        readonly NewRoomRequestHandler _handler;
        readonly Mock<IPeerConnectionFactory> _mockPeerConnectionFactory;
        readonly Mock<IRoom> _room;

        public NewRoomRequestHandlerTests()
        {
            _dispatchQueue.Start();
            _mockPeerConnectionFactory = new Mock<IPeerConnectionFactory>();
            _mockPeerConnectionFactory
                .Setup(x => x.Create())
                .Returns(Mock.Of<IPeerConnection>());
            _room = new Mock<IRoom>();
            _room.Setup(x => x.DispatchQueue).Returns(_dispatchQueue);
            _room.Setup(x => x.PeerConnectionFactory).Returns(_mockPeerConnectionFactory.Object);
            _handler = new NewRoomRequestHandler(_dispatchQueue, _roomRepository.Object, _mockRoomFactory.Object);
        }

        [Fact]
        public async Task HandleAsync_RoomDoesNotExist_CreateRoomAndReturnSuccess()
        {
            _roomRepository.Setup(x => x.GetRoomById(RoomId.FromString("my bar"))).Returns((Room)null);
            _roomRepository.Setup(x => x.AddRoom(It.IsAny<IRoom>())).Verifiable();
            _mockRoomFactory.Setup(x => x.Create(It.IsAny<RoomId>())).Returns(_room.Object);

            await _handler.HandleAsync(Mock.Of<IRemoteDevice>(), new NewRoomRequest
            {
                NewRoomName = "my bar"
            });
            _roomRepository.Verify();
        }

        [Fact]
        public async Task HandleAsync_RoomAlreadyExist_WontCreateRoomButReturnSuccessToo()
        {
            _roomRepository
                .Setup(x => x.GetRoomById(RoomId.FromString("my bar")))
                .Returns(_room.Object);
            _roomRepository
                .Setup(x => x.AddRoom(It.IsAny<IRoom>()))
                .Throws<InvalidOperationException>();

            var roomId = await _handler.HandleAsync(Mock.Of<IRemoteDevice>(), new NewRoomRequest
            {
                NewRoomName = "my bar"
            });
            Assert.Equal(roomId, RoomId.FromString("my-bar"));
        }

        [Fact]
        public async Task HandleAsync_UnExpectedError_Rethrow()
        {
            _roomRepository
                .Setup(x => x.AddRoom(It.IsAny<Room>()))
                .Throws<ApplicationException>();

            await Assert.ThrowsAsync<ApplicationException>(async delegate
            {
                await _handler.HandleAsync(Mock.Of<IRemoteDevice>(), new NewRoomRequest
                {
                    NewRoomName = "my bar"
                });
            });
        }
    }
}
