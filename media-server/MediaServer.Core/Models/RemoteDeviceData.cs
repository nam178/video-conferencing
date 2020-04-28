using MediaServer.Models;
using MediaServer.WebRtc.Managed;
using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    public sealed class RemoteDeviceData
    {
        public IRoom Room { get; set; }

        public User User { get; set; }

        public Dictionary<TrackQuality, VideoSinkInfo> VideoSinks { get; } = new Dictionary<TrackQuality, VideoSinkInfo>();

        public RemoteDeviceData()
        {

        }

        public RemoteDeviceData(RemoteDeviceData other) // copy constructor
        {
            Copy(other, this);
        }

        public static void Copy(RemoteDeviceData src, RemoteDeviceData dest)
        {
            dest.Room = src.Room;
            dest.User = src.User;

            // Copy VideoSinks
            foreach(var kv in src.VideoSinks)
            {
                dest.VideoSinks[kv.Key] = kv.Value;
            }

            // Copy PeerConnections
            dest.PeerConnections.Clear();
            foreach(var t in src.PeerConnections)
            {
                dest.PeerConnections.Add(t);
            }
        }

        public IList<IPeerConnection> PeerConnections { get; } = new List<IPeerConnection>();
    }
}
