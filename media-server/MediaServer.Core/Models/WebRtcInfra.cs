using MediaServer.Common.Threading;
using MediaServer.Core.Services.MediaRouting;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Threading;

namespace MediaServer.Core.Models
{
    sealed class WebRtcInfra : IWebRtcInfra
    {
        readonly PeerConnectionFactory _peerConnectionFactory;

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

        public WebRtcInfra()
        {
            _peerConnectionFactory = new WebRtc.Managed.PeerConnectionFactory();
        }

        int _initialised = 0;

        public void Initialize()
        {
            if(Interlocked.CompareExchange(ref _initialised, 1, 0) == 0)
            {
                _peerConnectionFactory.Initialize();
                _signallingThread = new RtcThread2DispatchQueueAdapter(_peerConnectionFactory.SignallingThread);
            }
            else
            {
                throw new InvalidOperationException("Already initialised");
            }
        }

        public IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, IRoom room)
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
                _peerConnectionFactory, 
                new[] {
                    stunUrls
                });
        }

        public IVideoRouter CreateVideoRouter()
        {
            if(Interlocked.CompareExchange(ref _initialised, 0, 0) == 0)
            {
                throw new InvalidOperationException("Not initialised");
            }
            return new VideoRouter(_signallingThread, _peerConnectionFactory);
        }
    }
}
