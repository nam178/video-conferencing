using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Models;

namespace MediaServer.Core.Repositories
{
    /// <summary>
    /// Store rooms. Designed to be a singleton for the server.
    /// </summary>
    /// <remarks>Not thread safe. Access the servie via its dispatch queue.</remarks>
    public interface IRoomRepository
    {
        /// <summary>
        /// Create new room
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="System.ArgumentException">When the id is invalid</exception>
        /// <exception cref="System.InvalidOperationException">When room already exists</exception>
        /// <returns></returns>
        Room CreateRoom(RoomId id);
    }
}
