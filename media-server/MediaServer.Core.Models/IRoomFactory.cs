namespace MediaServer.Core.Models
{
    public interface IRoomFactory
    {
        IRoom Create(RoomId id);
    }
}
