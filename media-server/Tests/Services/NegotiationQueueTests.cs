﻿using MediaServer.Common.Patterns;
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Services.Negotiation;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Services
{
    public class NegotiationQueueTests : IDisposable
    {
        readonly Mock<INegotiationMessageSubscriber> _mockSubscriber = new Mock<INegotiationMessageSubscriber>();
        readonly Mock<IThread> _mockSignallingThread = new Mock<IThread>();
        readonly NegotiationQueue _queue;

        public NegotiationQueueTests()
        {
            _queue = new NegotiationQueue(new INegotiationMessageSubscriber[]
            {
                _mockSubscriber.Object,
            }, _mockSignallingThread.Object);

            _mockSignallingThread.Setup(x => x.IsCurrent).Returns(true);
            _mockSignallingThread
                .Setup(x => x.Post(It.IsAny<Action<object>>(), null))
                .Callback<Action<object>, object>((action, u) =>
                {
                    _ = Task.Run(() => action(u));
                });
            _mockSubscriber
                .Setup(x => x.CanHandle(It.IsAny<NegotiationMessage>()))
                .Returns(true);
        }

        [Fact]
        public void Stop_QueueAlreadyStopped_ThrowsException()
        {
            _queue.Stop();
            Assert.Throws<InvalidOperationException>(delegate
            {
                _queue.Enqueue(new NegotiationMessage(Mock.Of<IPeerConnection>()));
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
                    .Setup(x => x.Handle(It.IsAny<NegotiationMessage>(), It.IsAny<Observer>()))
                    .Callback<NegotiationMessage, Observer>((m, observer) =>
                    {
                        observer.Success();
                        wh.Set();
                    });
                _queue.Enqueue(new NegotiationMessage(Mock.Of<IPeerConnection>()));

                Assert.True(wh.WaitOne(TimeSpan.FromSeconds(15)));
            }
        }

        [Fact]
        public void Enqueue_PeerConnectionAlreadyBeingProcessed_MessageGetsRequeued()
        {
            var firstMessageProcessingStart = new ManualResetEvent(false);
            var secondMessageSubmitted = new ManualResetEvent(false);
            var secondMessageProcessingEnded = new ManualResetEvent(false);
            var p = Mock.Of<IPeerConnection>();
            var m1 = new NegotiationMessage(p);
            var m2 = new NegotiationMessage(p);

            // Mock processing first message:
            _mockSubscriber
                .Setup(x => x.Handle(m1, It.IsAny<Observer>()))
                .Callback<NegotiationMessage, Observer>((m, observer) =>
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
                .Setup(x => x.Handle(m2, It.IsAny<Observer>()))
                .Callback<NegotiationMessage, Observer>((m, observer) =>
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
            var m1 = new NegotiationMessage(Mock.Of<IPeerConnection>());
            var m2 = new NegotiationMessage(Mock.Of<IPeerConnection>());
            var m1ProcessingStarted = false;

            // Mock processing first message:
            _mockSubscriber
                .Setup(x => x.Handle(It.IsAny<NegotiationMessage>(), It.IsAny<Observer>()))
                .Callback<NegotiationMessage, Observer>(async (msg, observer) =>
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