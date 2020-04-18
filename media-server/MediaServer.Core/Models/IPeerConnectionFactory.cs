namespace MediaServer.Core.Models
{
    public interface IPeerConnectionFactory
    {
        void EnsureInitialised();

        IPeerConnection Create();
    }
}
