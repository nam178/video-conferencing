using MediaServer.Common.Patterns;
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation.MessageQueue;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Services
{
    public class NegotiationQueueTests : IDisposable
    {
        readonly Mock<IMessageSubscriber> _mockSubscriber = new Mock<IMessageSubscriber>();
        readonly Mock<IThread> _mockSignallingThread = new Mock<IThread>();
        readonly NegotiationQueue _queue;

        public NegotiationQueueTests()
        {
            _queue = new NegotiationQueue(new IMessageSubscriber[]
            {
                _mockSubscriber.Object,
            });

            _mockSignallingThread.Setup(x => x.IsCurrent).Returns(true);
            _mockSignallingThread
                .Setup(x => x.Post(It.IsAny<Action<object>>(), null))
                .Callback<Action<object>, object>((action, u) =>
                {
                    _ = Task.Run(() => action(u));
                });

            _mockSubscriber
                .Setup(x => x.CanHandle(It.IsAny<Message>()))
                .Returns(true);
        }

        [Fact]
        public void Stop_QueueAlreadyStopped_ThrowsException()
        {
            _queue.Stop();
            Assert.Throws<InvalidOperationException>(delegate
            {
                _queue.Enqueue(new Message(MockPeerConnection()));
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Enqueue_NotProcessingAnyPeerConnection_MessageGetsProcessedImmediately(int numMessages)
        {
            for(var i = 0; i < numMessages; i++)
            {
                var wh = new ManualResetEvent(false);
                _mockSubscriber
                    .Setup(x => x.Handle(It.IsAny<Message>(), It.IsAny<Callback>()))
                    .Callback<Message, Callback>((m, observer) =>
                    {
                        observer.Success();
                        wh.Set();
                    });
                _queue.Enqueue(new Message(MockPeerConnection()));

                Assert.True(wh.WaitOne(TimeSpan.FromSeconds(15)));
            }
        }

        [Fact]
        public void Enqueue_PeerConnectionAlreadyBeingProcessed_MessageGetsRequeued()
        {
            var firstMessageProcessingStart = new ManualResetEvent(false);
            var secondMessageSubmitted = new ManualResetEvent(false);
            var secondMessageProcessingEnded = new ManualResetEvent(false);
            var p = MockPeerConnection();
            var m1 = new Message(p);
            var m2 = new Message(p);

            // Mock processing first message:
            _mockSubscriber
                .Setup(x => x.Handle(m1, It.IsAny<Callback>()))
                .Callback<Message, Callback>((m, observer) =>
                {
                    // Signal to indicate that the first message has begun processing
                    firstMessageProcessingStart.Set();

                    // Wait for the second message to be submitted
                    if(false == secondMessageSubmitted.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        throw new InvalidProgramException();
                    }

                    // Pretend processing then complete the first message
                    Thread.Sleep(2);
                    observer.Success();
                });

            // Mock processing second message
            _mockSubscriber
                .Setup(x => x.Handle(m2, It.IsAny<Callback>()))
                .Callback<Message, Callback>((m, observer) =>
                {
                    observer.Success();
                    secondMessageProcessingEnded.Set();
                });
            _queue.Enqueue(m1);

            // Wait for processing if the first message to start
            if(false == firstMessageProcessingStart.WaitOne(TimeSpan.FromSeconds(5)))
                throw new InvalidProgramException();

            // Submit the second message
            _queue.Enqueue(m2);
            secondMessageSubmitted.Set();

            // Wait fot second message to be processed
            Assert.True(secondMessageProcessingEnded.WaitOne(TimeSpan.FromSeconds(10)));
        }

        [Fact]
        public void Enqueue_2MessagesFromDifferentPeer_BothMessagesProcessedImmediately()
        {
            var m1Processed = new ManualResetEvent(false);
            var m2Processed = new ManualResetEvent(false);
            var m1 = new Message(MockPeerConnection());
            var m2 = new Message(MockPeerConnection());
            var m1ProcessingStarted = false;

            // Mock processing first message:
            _mockSubscriber
                .Setup(x => x.Handle(It.IsAny<Message>(), It.IsAny<Callback>()))
                .Callback<Message, Callback>(async (msg, observer) =>
                {
                    if(msg == m1)
                        m1ProcessingStarted = true;
                    // To proof that messages processed concurrently,
                    // We prove that their processing time overlaps.
                    // When message2 processed, message1 has started but yet completed
                    else
                    {
                        Assert.True(m1ProcessingStarted);
                        Assert.False(m1Processed.WaitOne(0));
                    }

                    // Pretend doing some works asynchronously
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Then callback upon completion
                    observer.Success();
                    if(msg == m1)
                        m1Processed.Set();
                    else
                        m2Processed.Set();
                });

            _queue.Enqueue(m1);
            _queue.Enqueue(m2);

            Assert.True(m1Processed.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.True(m2Processed.WaitOne(TimeSpan.FromSeconds(5)));
        }

        IPeerConnection MockPeerConnection()
        {

            var _mockPeerConnection = new Mock<IPeerConnection>();
            _mockPeerConnection.Setup(x => x.Room).Returns(delegate
            {
                var room = new Mock<IRoom>();
                room.Setup(x => x.SignallingThread).Returns(_mockSignallingThread.Object);
                return room.Object;
            });
            return _mockPeerConnection.Object;
        }

        public void Dispose()
        {
            try
            {
                _queue.Stop();
            }
            catch { }
            _queue.Dispose();
        }
    }
}