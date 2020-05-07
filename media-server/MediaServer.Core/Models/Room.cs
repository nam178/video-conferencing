using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.WebRtc.MediaRouting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServer.Models
{
    sealed class Room : IRoom
    {
        readonly IUserProfileCollection _userProfiles = new UserProfileCollection();
        readonly IWebRtcInfra _infra;

        public IDispatchQueue SignallingThread
        {
            get
            {
                CheckState();
                return _infra.SignallingThread;
            }
        }

        public RoomId Id { get; }

        public IUserProfileCollection UserProfiles
        {
            get
            {
                CheckState();
                return _userProfiles;
            }
        }

        public IWebRtcInfra PeerConnectionFactory
        {
            get
            {
                CheckState();
                return _infra;
            }
        }

        public IVideoRouter VideoRouter
        {
            get
            {
                CheckState();
                return _infra.VideoRouter;
            }
        }

        public RoomState State { get; private set; } = RoomState.JustCreated;

        public IDispatchQueue RenegotiationQueue { get; } = new ThreadPoolDispatchQueue(started: true);

        public Room(RoomId id, IWebRtcInfra peerConnectionFactory)
        {
            Require.NotEmpty(id);
            _infra = peerConnectionFactory
                ?? throw new System.ArgumentNullException(nameof(peerConnectionFactory));
            Id = id;
        }

        int _initLock = 0;

        public void Initialize()
        {
            if(Interlocked.CompareExchange(ref _initLock, 1, 0) != 0)
            {
                throw new InvalidOperationException();
            }
            State = RoomState.Initialising;
            _infra.Initialize();
            State = RoomState.Ok;
        }

        public Task<IPeerConnection> CreatePeerConnectionAsync(IRemoteDevice remoteDevice)
        {
            if(remoteDevice is null)
                throw new ArgumentNullException(nameof(remoteDevice));
            CheckState();
            return _infra.CreatePeerConnectionAsnc(remoteDevice, this);
        }

        public override string ToString() => $"[Room Id={Id}]";

        void CheckState()
        {
            if(State != RoomState.Ok)
                throw new InvalidOperationException();
        }
    }
}
