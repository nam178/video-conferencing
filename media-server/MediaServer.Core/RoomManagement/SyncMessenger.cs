using MediaServer.Models;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManagement
{
    sealed class SyncMessenger : ISyncMessenger
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task SendAsync(IRoom room)
        {
            if(room is null)
                throw new System.ArgumentNullException(nameof(room));
            SyncMessage syncMessage = default;
            IEnumerable<IRemoteDevice> destinationDevices = default;

            // Use the signalling thread to generate message 
            // becase the room model is not thread safe.
            // Also, this ensures newer updates send after older updates.
            await room.SignallingThread.ExecuteAsync(delegate
            {
                syncMessage = new SyncMessage();
                syncMessage.Users = room.UserProfiles.Select(u => new SyncMessage.UserInfo
                {
                    Id = u.Id,
                    Username = u.Username,
                    Devices = u.Devices.Select(d => new SyncMessage.DeviceInfo
                    {
                        DeviceId = d.Id
                    }).ToArray()
                }).ToArray();
                destinationDevices = room.UserProfiles.SelectMany(user => user.Devices).ToList();

                // Each device has it own message queue, 
                // so it's OK for dumping the messages to the devices all together
                foreach(var device in destinationDevices)
                {
                    device.EnqueueMessage(syncMessage);
                }
            });
        }
    }
}
