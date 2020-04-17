using System;

namespace MediaServer.Rtc.Models
{
    interface IPeerConnectionFactory
    {
        IPeerConnection Create(Guid id);
    }
}
