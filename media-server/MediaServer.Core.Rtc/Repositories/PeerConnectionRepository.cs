using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Rtc.Models;
using System;
using System.Collections.Generic;

namespace MediaServer.Rtc.Repositories
{
    sealed class PeerConnectionRepository : IPeerConnectionRepository
    {
        readonly Dictionary<UserProfile, List<IPeerConnection>> _records = new Dictionary<UserProfile, List<IPeerConnection>>();

        public void Add(UserProfile user, IPeerConnection peerConnection)
        {
            Require.NotNull(peerConnection);

            if(!_records.ContainsKey(user))
            {
                _records[user] = new List<IPeerConnection>
                {
                    peerConnection
                };
            }
            else
            {
                if(_records[user].Contains(peerConnection))
                {
                    throw new InvalidOperationException("Provided PeerConnection already exist");
                }
                _records[user].Add(peerConnection);
            }
        }

        public IReadOnlyList<IPeerConnection> Find(UserProfile user)
        {
            Require.NotNull(user);

            if(_records.ContainsKey(user))
            {
                return _records[user];
            }
            return _empty;
        }

        static readonly IReadOnlyList<IPeerConnection> _empty = new List<IPeerConnection>();
    }
}
