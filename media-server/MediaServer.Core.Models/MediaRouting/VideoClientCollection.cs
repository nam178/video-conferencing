using MediaServer.Common.Media;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Models.MediaRouting
{
    sealed class VideoClientCollection
    {
        readonly Dictionary<Guid, Client> _indexById;

        public VideoClientCollection()
        {
            _indexById = new Dictionary<Guid, Client>();
        }

        public Client Add(IRemoteDevice remoteDevice)
        {
            if(remoteDevice is null)
                throw new ArgumentNullException(nameof(remoteDevice));
            if(_indexById.ContainsKey(remoteDevice.Id))
                throw new InvalidOperationException($"VideoClient with id {remoteDevice.Id} already added.");
            var videoClient = new Client(remoteDevice);
            _indexById[remoteDevice.Id] = videoClient;
            return videoClient;
        }

        public VideoSource CreateVideoSource(Guid videoClientId, MediaQuality mediaQuality)
        {
            ThrowWhenKeyNotExist(videoClientId);
            if(_indexById[videoClientId].VideoSources.ContainsKey(mediaQuality))
            {
                throw new InvalidOperationException();
            }
            var t = new VideoSource(_indexById[videoClientId], mediaQuality);
            _indexById[videoClientId].VideoSources[mediaQuality] = t;
            return t;
        }

        public Client GetOrThrow(Guid videoClientId)
        {
            ThrowWhenKeyNotExist(videoClientId);
            return _indexById[videoClientId];
        }

        public Client FindByObserver(PeerConnectionObserver observer, out PeerConnection peerConnection)
        {
            foreach(var kv in _indexById)
            {
                for(var i = 0; i < kv.Value.PeerConnections.Count; i++)
                {
                    if(kv.Value.PeerConnections[i].Observer == observer)
                    {
                        peerConnection = kv.Value.PeerConnections[i];
                        return kv.Value;
                    }
                }
            }
            peerConnection = null;
            return null;
        }

        public IEnumerable<Client> OtherThan(Client videoClient) => _indexById.Where(kv => kv.Value != videoClient).Select(kv => kv.Value);

        public void Remove(IRemoteDevice remoteDevice)
        {
            if(_indexById.ContainsKey(remoteDevice.Id))
            {
                _indexById.Remove(remoteDevice.Id);
            }
        }

        void ThrowWhenKeyNotExist(Guid videoClientId)
        {
            if(!_indexById.ContainsKey(videoClientId))
                throw new InvalidOperationException($"VideoClient {videoClientId} has not been added.");
        }

        public override string ToString() => $"[VideoClientCollection Total Clients={_indexById.Count}]";
    }
}
