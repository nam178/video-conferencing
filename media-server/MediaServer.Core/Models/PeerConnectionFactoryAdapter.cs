using MediaServer.Common.Threading;
using MediaServer.Core.Services.RoomManager;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading;

namespace MediaServer.Core.Models
{
    sealed class PeerConnectionFactoryAdapter : IPeerConnectionFactory
    {
        readonly WebRtc.Managed.PeerConnectionFactory _impl;

        IDispatchQueue _signallingThread;
        public IDispatchQueue SignallingThread
        {
            get
            {
                if(Interlocked.CompareExchange(ref _initialised, 0, 0) == 0)
                {
                    throw new InvalidOperationException("Not initialised");
                }
                return _signallingThread;
            }
        }

        public PeerConnectionFactoryAdapter()
        {
            _impl = new WebRtc.Managed.PeerConnectionFactory();
        }

        int _initialised = 0;

        public void Initialize()
        {
            if(Interlocked.CompareExchange(ref _initialised, 1, 0) == 0)
            {
                _impl.Initialize();
                _signallingThread = new RtcThread2DispatchQueueAdapter(_impl.SignallingThread);
            }
            else
            {
                throw new InvalidOperationException("Already initialised");
            }
        }

        public IPeerConnection Create(IRemoteDevice remoteDevice, IRoom room)
        {
            if(remoteDevice is null)
                throw new ArgumentNullException(nameof(remoteDevice));
            if(room is null)
                throw new ArgumentNullException(nameof(room));

            var stunUrls = Environment.GetEnvironmentVariable("STUN_URLS");
            if(string.IsNullOrWhiteSpace(stunUrls))
            {
                throw new ApplicationException("STUN_URLS environment variable has not been set");
            }
            return new PeerConnectionAdapter(
                room,
                remoteDevice,
                _impl, 
                new[] {
                    stunUrls
                });
        }

        public IVideoRouter CreateVideoRouter(Room room)
        {
            return new VideoRouter(room, _impl);
        }
    }
}
