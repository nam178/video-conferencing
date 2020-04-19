using MediaServer.Common.Threading;
using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;

namespace MediaServer.Models
{
    sealed class Room : IRoom
    {
        public IDispatchQueue DispatchQueue { get; }

        public RoomId Id { get; set; }

        public IUserProfileCollection UserProfiles { get; } = new UserProfileCollection();

        public IPeerConnectionFactory PeerConnectionFactory { get; }

        public Room(RoomId id, IPeerConnectionFactory peerConnectionFactory)
        {
            Require.NotEmpty(id);
            PeerConnectionFactory = peerConnectionFactory
                ?? throw new System.ArgumentNullException(nameof(peerConnectionFactory));
            DispatchQueue = new ThreadPoolDispatchQueue();
            Id = id;
            ((ThreadPoolDispatchQueue)DispatchQueue).Start();
        }

        public override string ToString() => $"[Room Id={Id}]";
    }
}
