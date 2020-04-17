namespace MediaServer.Rtc.Models
{
    interface IPeerConnectionFactory
    {
        IPeerConnection Create();
    }
}
