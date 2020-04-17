using MediaServer.WebRtc.Managed;
using System;

namespace MediaServer.Core.Rtc.Models
{
    interface IPeerConnection : IDisposable
    {
        public RTCSessionDescription RemoteSessionDescription { get; set; }
    }
}
