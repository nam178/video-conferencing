using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using System;
using System.Threading;

namespace MediaServer.Models
{
    sealed class Room : IRoom
    {
        readonly IUserProfileCollection _userProfiles = new UserProfileCollection();
        readonly IPeerConnectionFactory _peerConnectionFactory;

        public IDispatchQueue SignallingThread
        {
            get
            {
                CheckState();
                return _peerConnectionFactory.SignallingThread;
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

        public IPeerConnectionFactory PeerConnectionFactory
        {
            get
            {
                CheckState();
                return _peerConnectionFactory;
            }
        }

        public RoomState State { get; private set; } = RoomState.JustCreated;

        public Room(RoomId id, IPeerConnectionFactory peerConnectionFactory)
        {
            Require.NotEmpty(id);
            _peerConnectionFactory = peerConnectionFactory
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
            _peerConnectionFactory.Initialize();
            State = RoomState.Ok;
        }

        public override string ToString() => $"[Room Id={Id}]";

        void CheckState()
        {
            if(State != RoomState.Ok)
                throw new InvalidOperationException();
        }
    }
}
