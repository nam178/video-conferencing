﻿using MediaServer.Common.Patterns;
using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.WebRtc.Managed;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MediaServer.Core.Services.Negotiation
{
    sealed class NegotiationQueue : IDisposable
    {
        readonly IReadOnlyList<INegotiationMessageSubscriber> _subscribers;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        readonly IThread _signallingThread;
        readonly Thread _dequeueThread;
        readonly ConcurrentQueue<NegotiationMessage> _messages = new ConcurrentQueue<NegotiationMessage>();
        readonly AutoResetEvent _backgroundThreadWakeUpSignal = new AutoResetEvent(false);
        readonly ManualResetEvent _backgroundThreadCompletionSignal = new ManualResetEvent(false);
        readonly Dictionary<IPeerConnection, Queue<NegotiationMessage>> _peerConnectionQueue = new Dictionary<IPeerConnection, Queue<NegotiationMessage>>();

        int _state;

        public NegotiationQueue(
            IEnumerable<INegotiationMessageSubscriber> subscribers,
            IThread signallingThread)
        {
            _subscribers = (subscribers
                ?? throw new ArgumentNullException(nameof(subscribers))).ToList();
            _dequeueThread = new Thread(DequeueThread);
            _dequeueThread.Name = "Dequeue Thread";
            _dequeueThread.Start();
            _signallingThread = signallingThread
                ?? throw new ArgumentNullException(nameof(signallingThread));
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
                                _peerConnectionQueue[message.PeerConnection] = new Queue<NegotiationMessage>();
                        }

                        // At this point, this PeerConnection is free to be processed.
                        // Any further request to process is PeerConnection will be put into its own queue,
                        // and requeued later.
                        var subscriber = GetSubscriber(message);
                        _signallingThread.Post(delegate
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
        public void Enqueue(NegotiationMessage message)
        {
            var state = Interlocked.CompareExchange(ref _state, 0, 0);
            if(state == 1)
                throw new InvalidOperationException("Already stopped");
            else if(state == 2)
                throw new ObjectDisposedException(GetType().FullName);

            var subscriber = GetSubscriber(message);
            if(subscriber != null)
            {
                _messages.Enqueue(message);
                _backgroundThreadWakeUpSignal.Set();
            }
            else throw new ArgumentException($"No subscriber could handle message {message.GetType().FullName}");
        }

        INegotiationMessageSubscriber GetSubscriber(NegotiationMessage message)
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
        void OnProcessingComplete(NegotiationMessage message)
        {
            _signallingThread.EnsureCurrentThread();

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
