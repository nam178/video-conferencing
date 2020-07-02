namespace MediaServer.Core.Models.Repositories
{
    /// <summary>
    /// Store rooms. Designed to be a singleton for the server.
    /// </summary>
    /// <remarks>Not thread safe. Access the servie via its dispatch queue.</remarks>
    public interface IRoomRepository
    {
        /// <summary>
        /// Add specified room into the repo
        /// </summary>
        /// <param name="room"></param>
        /// <exception cref="System.InvalidOperationException">When room already exist</exception>
        void AddRoom(IRoom room);

        /// <summary>
        /// Find the specified room
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The room if found, or NULL when the room doesn't exist</returns>
        IRoom GetRoomById(RoomId id);
    }
}
