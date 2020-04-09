using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Models
{
    sealed class Room
    {
        IReadOnlyList<Peer> _peers = new List<Peer>();
        readonly ManualResetEvent _evt = new ManualResetEvent(false);
        readonly object _syncRoot = new object();

        public event EventHandler<EventArgs<Peer>> PeerAdded;

        public IReadOnlyList<Peer> Peers => _peers;

        bool _isInitialised = false;
        public bool IsInitialised 
        { 
            get => _isInitialised; 
            set {
                if(value == false)
                {
                    throw new ArgumentException();
                }
                _isInitialised = value;
                _evt.Set();
            } 
        }

        /// <summary>
        /// Each room has its own dispatch queue, used to update models, handling signals, etc..
        /// to avoid race conditions.
        /// </summary>
        public IDispatchQueue DispatchQueue { get; }

        public Task WaitForInitialisationAsync() => Task.Run(() => _evt.WaitOne());

        public Room()
        {
            // Create and start an exclusive dispatch queue for this room
            DispatchQueue = new DispatchQueue();
            ((DispatchQueue)DispatchQueue).Start();
        }

        public void AddPeer(Peer peer)
        {
            CopyOnWrite.Add(ref _peers, peer, _syncRoot);
            PeerAdded?.Invoke(this, new EventArgs<Peer>(peer));
        }
    }
}
