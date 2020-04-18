using MediaServer.Common.Threading;
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

        public Room(IPeerConnectionFactory peerConnectionFactory)
        {
            PeerConnectionFactory = peerConnectionFactory
                ?? throw new System.ArgumentNullException(nameof(peerConnectionFactory));
            DispatchQueue = new ThreadPoolDispatchQueue();
            ((ThreadPoolDispatchQueue)DispatchQueue).Start();
        }

        public override string ToString() => $"[Room Id={Id}]";
    }
}
