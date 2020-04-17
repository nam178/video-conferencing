namespace MediaServer.Core.Models
{
    interface IPeerConnectionFactory
    {
        IPeerConnection Create();
    }
}
