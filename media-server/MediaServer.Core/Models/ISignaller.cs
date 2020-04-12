using System.Threading.Tasks;

namespace MediaServer.Models
{
    public interface ISignaller
    {
        Task SendAsync(string command, object args);
    }

    static class ISignallerExtensions
    {
        public static Task SendMessageAsync(this ISignaller peerMessenger, string command)
        {
            return peerMessenger.SendAsync(command, null);
        }
    }
}