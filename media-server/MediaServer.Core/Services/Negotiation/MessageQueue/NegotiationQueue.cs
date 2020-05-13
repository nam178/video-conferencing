using MediaServer.Common.Patterns;
using MediaServer.Core.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    sealed class NegotiationQueue : IDisposable
    {
        readonly IReadOnlyList<IMessageSubscriber> _subscribers;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly Thread _dequeueThread;
        readonly ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();
        readonly AutoResetEvent _backgroundThreadWakeUpSignal = new AutoResetEvent(false);
        readonly ManualResetEvent _backgroundThreadCompletionSignal = new ManualResetEvent(false);
        readonly Dictionary<IPeerConnection, Queue<Message>> _peerConnectionQueue = new Dictionary<IPeerConnection, Queue<Message>>();

        int _state;

        public NegotiationQueue(IEnumerable<IMessageSubscriber> subscribers)
        {
            _subscribers = (subscribers
                ?? throw new ArgumentNullException(nameof(subscribers))).ToList();
            _dequeueThread = new Thread(DequeueThread);
            _dequeueThread.Name = "Dequeue Thread";
            _dequeueThread.Start();
        }

        void DequeueThread()
        {
            try
            {
                while(Interlocked.CompareExchange(ref _state, 0, 0) == 0)
                {
                    _backgroundThreadWakeUpSignal.WaitOne(TimeSpan.FromSeconds(5));
                    // Stopped?
                    if(Interlocked.CompareExchange(ref _state, 0, 0) != 0)
                    {
                        _logger.Debug("Cancelling dequeue thread..");
                        return;
                    }
                    // Dequeue a message
                    if(_messages.TryDequeue(out var message))
                    {
                        // We can only process 1 PeerConnection at a time,
                        // if this PeerConnection being processed, queue this message so it gets processed later.
                        lock(_peerConnectionQueue)
                        {
                            var isBeingProcessed = _peerConnectionQueue.ContainsKey(message.PeerConnection);
                            if(isBeingProcessed)
                            {
                                _peerConnectionQueue[message.PeerConnection].Enqueue(message);
                                continue;
                            }
                            else
                                _peerConnectionQueue[message.PeerConnection] = new Queue<Message>();
                        }

                        // At this point, this PeerConnection is free to be processed.
                        // Any further request to process is PeerConnection will be put into its own queue,
                        // and requeued later.
                        var subscriber = GetSubscriber(message);
                        var signallingThread = message.PeerConnection.Room.SignallingThread;
                        signallingThread.Post(delegate
                        {
                            subscriber.Handle(
                                message,
                                new Observer()
                                    .OnError((err) =>
                                    {
                                        _logger.Warn(err);
                                        OnProcessingComplete(message);
                                    })
                                    .OnSuccess(() => OnProcessingComplete(message)));
                        });
                    }
                }
            }
            finally
            {
                _backgroundThreadCompletionSignal.Set();
                _logger.Debug("Dequeue thread stopped");
            }
        }

        /// <summary>
        /// Stop processing messages. Must call this before disposing.
        /// </summary>
        /// <remarks>This method is thread safe. Does not race with Enqueue()</remarks>
        public void Stop()
        {
            // Flag that we're stopped
            if(Interlocked.CompareExchange(ref _state, 1, 0) == 0)
            {
                // Signal the background thread to wake up,
                // so it cak back for _stoppedFlag
                _backgroundThreadWakeUpSignal.Set();
                // Wait until the background thread is fully stopped
                _backgroundThreadCompletionSignal.WaitOne();
            }
            else throw new InvalidOperationException();
        }

        /// <summary>
        /// Enqueue a message for processsing
        /// </summary>
        /// <remarks>This method is thread safe. Does not race with Enqueue()</remarks>
        /// <param name="message"></param>
        public void Enqueue(Message message)
        {
            var state = Interlocked.CompareExchange(ref _state, 0, 0);
            if(state == 1)
                throw new InvalidOperationException("Already stopped");
            else if(state == 2)
                throw new ObjectDisposedException(GetType().FullName);
            if(message.PeerConnection == null)
                throw new ArgumentException($"{nameof(message.PeerConnection)} is NULL");
            if(message.PeerConnection.Room == null)
                throw new InvalidProgramException($"{nameof(message.PeerConnection.Room)} is NULL");

            var subscriber = GetSubscriber(message);
            if(subscriber != null)
            {
                _messages.Enqueue(message);
                _backgroundThreadWakeUpSignal.Set();
            }
            else throw new ArgumentException($"No subscriber could handle message {message.GetType().FullName}");
        }

        IMessageSubscriber GetSubscriber(Message message)
        {
            for(var i = 0; i < _subscribers.Count; i++)
            {
                if(_subscribers[i].CanHandle(message))
                {
                    return _subscribers[i];
                }
            }
            return null;
        }

        // Callback that triggered whenever the processing of a message is completed.
        // We'll re-queue any messages that was queued during the processing of this message.
        void OnProcessingComplete(Message message)
        {
            lock(_peerConnectionQueue)
            {
                if(false == _peerConnectionQueue.ContainsKey(message.PeerConnection))
                    throw new InvalidProgramException("Possibly that observer callback called more than once");
                var queue = _peerConnectionQueue[message.PeerConnection];
                while(queue.Count > 0)
                {
                    Enqueue(queue.Dequeue());
                }
                _peerConnectionQueue.Remove(message.PeerConnection);
            }
        }

        public void Dispose()
        {
            if(Interlocked.CompareExchange(ref _state, 2, 1) == 1)
            {
                _backgroundThreadWakeUpSignal.Dispose();
                _backgroundThreadCompletionSignal.Dispose();
            }
        }
    }
}
