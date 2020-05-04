using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using MediaServer.WebRtc.MediaRouting;
using System;
using System.Threading;

namespace MediaServer.Core.Adapters
{
    sealed class WebRtcInfraAdapter : IWebRtcInfra
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

        VideoRouter _videoRouter;
        public IVideoRouter VideoRouter
        {
            get
            {
                RequireInitialised();
                return _videoRouter;
            }
        }

        public WebRtcInfraAdapter()
        {
            _peerConnectionFactory = new PeerConnectionFactory();
        }

        int _initialised = 0;
        public void Initialize()
        {
            if(Interlocked.CompareExchange(ref _initialised, 1, 0) == 0)
            {
                _peerConnectionFactory.Initialize();
                _signallingThread = new RtcThread2DispatchQueueAdapter(_peerConnectionFactory.SignallingThread);
                _videoRouter = new VideoRouter(_signallingThread, _peerConnectionFactory);
            }
            else
            {
                throw new InvalidOperationException("Already initialised");
            }
        }

        public IPeerConnection CreatePeerConnection(IRemoteDevice remoteDevice, IRoom room)
        {
            RequireInitialised();
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
                new[] { stunUrls },
                _videoRouter);
        }

        void RequireInitialised()
        {
            if(Interlocked.CompareExchange(ref _initialised, 0, 0) != 1)
                throw new InvalidOperationException("Not Initialised");
        }
    }
}
