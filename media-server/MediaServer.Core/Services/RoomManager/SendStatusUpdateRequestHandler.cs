using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Models;
using NLog;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    /// <summary>
    /// Handles the request where member status update need to be sent to all connected devices
    /// </summary>
    sealed class SendStatusUpdateRequestHandler : IHandler<SendStatusUpdateRequest>
    {
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task HandleAsync(SendStatusUpdateRequest request)
        {
            // Jump into the room and get all the devices and generate an update message
            var updateMessage = new RemoteDeviceUserUpdateMessage { };
            var devices = await request.Room.DispatchQueue.ExecuteAsync(delegate
            {
                updateMessage.Users = request.Room.UserProfiles.Select(u => new RemoteDeviceUserUpdateMessage.UserProfile
                {
                    Id = u.Id,
                    Username = u.Username,
                    IsOnline = u.Devices.Any()
                }).ToArray();
                return request.Room.UserProfiles.SelectMany(user => user.Devices).ToList();
            });

            // Each device has it own message queue, 
            // so it's OK for dumping the messages to the devices all together
            foreach(var device in devices)
            {
                Send(device, updateMessage);
            }
        }

        void Send(IRemoteDevice device, RemoteDeviceUserUpdateMessage message)
        {
            device
                .SendUserUpdateAsync(message)
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
