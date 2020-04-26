using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Core.Repositories;
using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{

    sealed class VideoRouter // video routing for a room
    {
        readonly Room _room;
        readonly IDispatchQueue _centralDispatchQueue;
        readonly IPeerConnectionRepository _peerConnectionRepository;
        readonly Dictionary<Guid, Dictionary<TrackQuality, PassiveVideoTrackSource>> _sources;

        public VideoRouter(Room room, IDispatchQueue centralDispatchQueue, IPeerConnectionRepository peerConnectionRepository)
        {
            _room = room
                ?? throw new System.ArgumentNullException(nameof(room));
            _centralDispatchQueue = centralDispatchQueue 
                ?? throw new ArgumentNullException(nameof(centralDispatchQueue));
            _peerConnectionRepository = peerConnectionRepository
                ?? throw new ArgumentNullException(nameof(peerConnectionRepository));
            _sources = new Dictionary<Guid, Dictionary<TrackQuality, PassiveVideoTrackSource>>();
        }

        /// <summary>
        /// Notify this router that a device joined the provided room
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Should be called from the device messaging thread</remarks>
        /// <returns></returns>
        public Task DeviceJoinedRoom(IRemoteDevice device)
        {
            return _room.DispatchQueue.ExecuteAsync(delegate
            {
                if(_sources.ContainsKey(device.Id))
                    throw new InvalidOperationException("Device already joined the room");
                _sources[device.Id] = new Dictionary<TrackQuality, PassiveVideoTrackSource>();
                foreach(var quality in new[] { TrackQuality.High, TrackQuality.Mid, TrackQuality.Low })
                {
                    _sources[device.Id][quality] = new PassiveVideoTrackSource();
                }
            });
        }

        /// <summary>
        /// Notify this router that a device left the provided room
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Should be called from the device messaging thread</remarks>
        /// <returns></returns>
        public Task DeviceLeftRoom(IRemoteDevice device)
        {
            return _room.DispatchQueue.ExecuteAsync(delegate
            {
                // Dispose all the track sources
                if(_sources.ContainsKey(device.Id))
                {
                    foreach(var t in _sources[device.Id])
                    {
                        t.Value.Dispose();
                    }
                }
            });
        }

        /// <summary>
        /// Notify this router that a remote track has been added 
        /// </summary>
        /// <param name="device"></param>
        /// <remarks>Must be called from signalling thread</remarks>
        /// <returns></returns>
        public void TrackAdded(IPeerConnection peerConnection, RtpReceiver rtpReceiver)
        {
            throw new NotImplementedException();
        }
    }
}
