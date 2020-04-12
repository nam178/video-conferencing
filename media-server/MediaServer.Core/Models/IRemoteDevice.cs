using System.Threading.Tasks;

namespace MediaServer.Models
{
    /// <summary>
    /// Represents a remote device that connected to us.
    /// For instance it's a client connected via websocket, however we don't care how they connected,
    /// we see them dumb "devices".
    /// </summary>
    public interface IRemoteDevice
    {
        /// <summary>
        /// Send an user-update message to this device
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.Exception">When network error or things like that occurs</exception>
        /// <returns></returns>
        Task SendUserUpdateAsync(RemoteDeviceUserUpdateMessage message);

        /// <summary>
        /// Terminate the connection with this device.
        /// Implementation must not throw exception.
        /// </summary>
        void Teminate();
    }
}