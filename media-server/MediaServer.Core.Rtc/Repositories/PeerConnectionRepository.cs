using MediaServer.Core.Models;
using MediaServer.Models;
using MediaServer.Rtc.Models;
using System;
using System.Collections.Generic;

namespace MediaServer.Rtc.Repositories
{
    sealed class PeerConnectionRepository : IPeerConnectionRepository
    {
        // Keeping records of user/devie -> peer connections
        readonly Dictionary<UserProfile, (IRemoteDevice, IPeerConnection)> _records = new Dictionary<UserProfile, (IRemoteDevice, IPeerConnection)>();

        public void Add(UserProfile user, IRemoteDevice remoteDevice, IPeerConnection peerConnection)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyList<IPeerConnection> Find(IRemoteDevice remoteDevice)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(IPeerConnection peerConnection)
        {
            throw new NotImplementedException();
        }
    }
}
