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
        Task SendAsync(string command, object args);
    }

    static class ISignallerExtensions
    {
        public static Task SendMessageAsync(this IRemoteDevice peerMessenger, string command)
        {
            return peerMessenger.SendAsync(command, null);
        }
    }
}