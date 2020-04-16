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
        readonly NewRoomRequestHandler _handler;

        public NewRoomRequestHandlerTests()
        {
            _dispatchQueue.Start();
            _handler = new NewRoomRequestHandler(_dispatchQueue, _roomRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_RoomNotCreated_CreateRoomAndReturnSuccess()
        {
            _roomRepository
                .Setup(x => x.CreateRoom(It.IsAny<RoomId>()))
                .Returns(new Room
                {
                    Id = RoomId.FromString("my-bar")
                })
                .Verifiable();

            var result = await _handler.HandleAsync(Mock.Of<IRemoteDevice>(), new NewRoomRequest
            {
                NewRoomName = "my bar"
            });
            Assert.True(result.Success);
            _roomRepository.Verify();
        }

        [Fact]
        public async Task HandleAsync_RoomAlreadyExist_WontCreateRoomButReturnSuccessToo()
        {
            _roomRepository
                .Setup(x => x.CreateRoom(It.IsAny<RoomId>()))
                .Throws<InvalidOperationException>();

            var result = await _handler.HandleAsync(Mock.Of<IRemoteDevice>(), new NewRoomRequest
            {
                NewRoomName = "my bar"
            });
            Assert.True(result.Success);
            Assert.Equal(result.CreatedRoomId, RoomId.FromString("my-bar"));
        }

        [Fact]
        public async Task HandleAsync_UnExpectedError_ReturnsFailure()
        {
            _roomRepository
                .Setup(x => x.CreateRoom(It.IsAny<RoomId>()))
                .Throws<ApplicationException>();

            var result = await _handler.HandleAsync(Mock.Of<IRemoteDevice>(), new NewRoomRequest
            {
                NewRoomName = "my bar"
            });
            Assert.False(result.Success);
        }
    }
}
