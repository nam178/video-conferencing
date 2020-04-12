using MediaServer.Common.Mediator;
using MediaServer.Common.Threading;
using MediaServer.Models;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Core.Services.RoomManager
{
    /// <summary>
    /// Handles the request where member status update need to be sent to all connected devices
    /// </summary>
    sealed class SendStatusUpdateRequestHandler : IHandler<SendStatusUpdateRequest>
    {
        readonly IParallelQueue _centralParallelQueue;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        const int MAX_WAIT_TIMEOUT_SECONDS = 5;

        public SendStatusUpdateRequestHandler(IParallelQueue centralParallelQueue)
        {
            _centralParallelQueue = centralParallelQueue
                ?? throw new System.ArgumentNullException(nameof(centralParallelQueue));
        }

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

            // Then use the central parallel queue to send the update
            foreach(var device in devices)
            {
                _centralParallelQueue.Enqueue(device, d =>
                {
                    var tmp = (IRemoteDevice)d;
                    try
                    {
                        if(false == tmp
                            .SendUserUpdateAsync(updateMessage)
                            .Wait(TimeSpan.FromSeconds(MAX_WAIT_TIMEOUT_SECONDS)))
                        {
                            throw new TimeoutException();
                        }
                        _logger.Trace($"User update sent to {device}");
                    }
                    catch(Exception ex)
                    {
                        _logger.Warn(ex, $"Failed sending notification to device {device}, terminating");
                        tmp.Teminate();
                        throw;
                    }
                });
            }
        }
    }
}
