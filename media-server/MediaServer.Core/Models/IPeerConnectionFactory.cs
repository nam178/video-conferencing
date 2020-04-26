using MediaServer.Models;

namespace MediaServer.Core.Models
{
    public interface IPeerConnectionFactory
    {
        void EnsureInitialised();

        IPeerConnection Create(IRemoteDevice remoteDevice, IRoom room);
    }
}
