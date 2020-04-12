using MediaServer.Common.Threading;
using MediaServer.Core.Models;
using MediaServer.Models;

namespace MediaServer.Core.Repositories
{
    /// <summary>
    /// Store rooms. Designed to be a singleton for the server.
    /// </summary>
    /// <remarks>Not thread safe. Access the servie via its dispatch queue.</remarks>
    interface IRoomRepository
    {
        /// <summary>
        /// Create new room
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="System.ArgumentException">When the id is invalid</exception>
        /// <exception cref="System.InvalidOperationException">When room already exists</exception>
        /// <returns></returns>
        Room CreateRoom(RoomId id);

        /// <summary>
        /// Find the specified room
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The room if found, or NULL when the room doesn't exist</returns>
        Room GetRoomById(RoomId id);
    }
}
