using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    public sealed class RemoteDeviceData
    {
        public IRoom Room { get; set; }

        public User User { get; set; }

        public IList<IPeerConnection> PeerConnections { get; } = new List<IPeerConnection>();

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

            // Copy PeerConnections
            dest.PeerConnections.Clear();
            foreach(var t in src.PeerConnections)
            {
                dest.PeerConnections.Add(t);
            }
        }
    }
}
