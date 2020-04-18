using MediaServer.Common.Utils;
using MediaServer.Core.Models;
using MediaServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Repositories
{
    sealed class PeerConnectionRepository : IPeerConnectionRepository
    {
        readonly Dictionary<UserProfile, List<(IRemoteDevice Device, IPeerConnection Pc)>> _indexByUser;
        readonly Dictionary<IRemoteDevice, List<IPeerConnection>> _indexByDevice;
        readonly Dictionary<IPeerConnection, (UserProfile User, IRemoteDevice Device)> _indexByPeerConnection;

        public PeerConnectionRepository()
        {
            _indexByDevice = new Dictionary<IRemoteDevice, List<IPeerConnection>>();
            _indexByUser = new Dictionary<UserProfile, List<(IRemoteDevice, IPeerConnection)>>();
            _indexByPeerConnection = new Dictionary<IPeerConnection, (UserProfile, IRemoteDevice)>();
        }

        public void Add(UserProfile user, IRemoteDevice remoteDevice, IPeerConnection peerConnection)
        {
            ThrowIfAlreadyAdded(user, remoteDevice, peerConnection);

            if(!_indexByUser.ContainsKey(user))
                _indexByUser[user] = new List<(IRemoteDevice, IPeerConnection)>();
            if(!_indexByDevice.ContainsKey(remoteDevice))
                _indexByDevice[remoteDevice] = new List<IPeerConnection>();
            _indexByUser[user].Add((remoteDevice, peerConnection));
            _indexByDevice[remoteDevice].Add(peerConnection);
            _indexByPeerConnection[peerConnection] = (user, remoteDevice);
        }

        void ThrowIfAlreadyAdded(UserProfile user, IRemoteDevice remoteDevice, IPeerConnection peerConnection)
        {
            Require.NotNull(user);
            Require.NotNull(remoteDevice);
            Require.NotNull(peerConnection);
            
            if(_indexByUser.ContainsKey(user)
                && _indexByUser[user].Any(record => record.Device == remoteDevice && record.Pc == peerConnection))
            {
                throw new InvalidOperationException(
                    $"User {user} already associated with PeerConnection " +
                    $"{peerConnection} and device {remoteDevice}");
            }
            if(_indexByDevice.ContainsKey(remoteDevice)
                && _indexByDevice[remoteDevice].Contains(peerConnection))
            {
                throw new InvalidOperationException(
                    $"Device {remoteDevice} already associated " +
                    $"with PeerConnection {peerConnection}");
            }
            if(_indexByPeerConnection.ContainsKey(peerConnection))
                throw new InvalidOperationException($"PeerConnection {peerConnection} already added");
        }

        public IReadOnlyList<IPeerConnection> Find(IRemoteDevice remoteDevice)
        {
            if(_indexByDevice.ContainsKey(remoteDevice))
            {
                return _indexByDevice[remoteDevice];
            }
            return _empty;
        }

        static readonly IReadOnlyList<IPeerConnection> _empty = new List<PeerConnectionAdapter>();

        public void Remove(IPeerConnection peerConnection)
        {
            Require.NotNull(peerConnection);

            if(!_indexByPeerConnection.ContainsKey(peerConnection))
                return;
            var tmp = _indexByPeerConnection[peerConnection];
            // Remove PeerConnection -> User/Device index
            _indexByPeerConnection.Remove(peerConnection);
            // Remove User -> PeerConnection/Device index
            if(_indexByUser.ContainsKey(tmp.User))
            {
                _indexByUser[tmp.User].RemoveAll(e => e.Pc == peerConnection);
                if(_indexByUser.Count == 0)
                {
                    _indexByUser.Remove(tmp.User);
                }
            }
            // Remove Device -> PeerConnection index
            if(_indexByDevice.ContainsKey(tmp.Device))
            {
                _indexByDevice[tmp.Device].RemoveAll(e => e == peerConnection);
                if(_indexByDevice.Count == 0)
                {
                    _indexByDevice.Remove(tmp.Device);
                }
            }    
        }
    }
}
