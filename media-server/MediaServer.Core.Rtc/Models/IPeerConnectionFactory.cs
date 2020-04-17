namespace MediaServer.Core.Rtc.Models
{
    interface IPeerConnectionFactory
    {
        IPeerConnection Create();
    }
}
