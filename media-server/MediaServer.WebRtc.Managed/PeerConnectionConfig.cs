using System.Collections.Generic;

namespace MediaServer.WebRtc.Managed
{
    public sealed class PeerConnectionConfig
    {
        public class IceServerInfo
        {
            public List<string> Urls { get; } = new List<string>();

            public string Username { get; set; }

            public string Password { get; set; }
        }

        public List<IceServerInfo> IceServers { get; } = new List<IceServerInfo>();
    }
}