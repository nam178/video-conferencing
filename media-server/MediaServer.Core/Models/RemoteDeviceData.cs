using MediaServer.Models;
using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    public sealed class RemoteDeviceData
    {
        public IRoom Room { get; set; }

        public User User { get; set; }

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
            dest.PeerConnections.Clear();
            foreach(var t in src.PeerConnections)
            {
                dest.PeerConnections.Add(t);
            }
        }

        public IList<IPeerConnection> PeerConnections { get; } = new List<IPeerConnection>();
    }
}
