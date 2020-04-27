using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Models;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    /// <summary>
    /// Handles the request where member status update need to be sent to all connected devices
    /// </summary>
    sealed class SendSyncMessageRequestHandler : IHandler<SendSyncMessageRequest>
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task HandleAsync(SendSyncMessageRequest request)
        {
            SyncMessage syncMessage = default;
            IEnumerable<IRemoteDevice> destinationDevices = default;

            // Jump into the room and get all the devices and generate an update message
            await request.Room.SignallingThread.ExecuteAsync(delegate
            {
                syncMessage = new SyncMessage();
                syncMessage.Users = request.Room.UserProfiles.Select(u => new SyncMessage.UserInfo
                {
                    Id = u.Id,
                    Username = u.Username,
                    Devices = u.Devices.Select(d => new SyncMessage.DeviceInfo
                    {
                        DeviceId = d.Id
                    }).ToArray()
                }).ToArray();
                destinationDevices = request.Room.UserProfiles.SelectMany(user => user.Devices).ToList();
            });

            // Each device has it own message queue, 
            // so it's OK for dumping the messages to the devices all together
            foreach(var device in destinationDevices)
            {
                Send(device, syncMessage);
            }
        }

        void Send(IRemoteDevice device, SyncMessage syncMessage)
        {
            device
                .SendSyncMessageAsync(syncMessage)
                .ContinueWith(task =>
                {
                    if(task.Exception != null)
                    {
                        _logger.Warn(task.Exception, $"Failed sending notification to device {device}, terminating");
                        device.Teminate();
                    }
                    else
                    {
                        _logger.Trace($"User update sent to {device}");
                    }
                })
                .Forget();
        }
    }
}
