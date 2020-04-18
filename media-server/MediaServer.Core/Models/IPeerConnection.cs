using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Models
{
    public interface IPeerConnection : IDisposable
    {
        public RTCSessionDescription RemoteSessionDescription { get; set; }
    }
}
