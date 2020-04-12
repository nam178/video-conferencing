using MediaServer.Common.Commands;
using MediaServer.Models;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class AddIceCandidateHandler : ICommandHandler<WebSocketClient, AddIceCandidate>
    {
        readonly Room _room;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public AddIceCandidateHandler(Room channel)
        {
            _room = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public async Task HandleAsync(WebSocketClient arg1, AddIceCandidate arg2)
        {
            if(!_room.IsInitialised)
            {
                throw new InvalidOperationException($"The room must be initialised first");
            }

            await _room.DispatchQueue.ExecuteAsync(delegate
            {
                try
                {
                    _room.Peers.First().ReceiveRemoteCandidate(arg2.Candidate);
                }
                catch(Exception ex)
                {
                    _logger.Fatal(ex);
                    throw new NotImplementedException("TODO - handle adding candidate failure");
                }
            });
        }
    }
}
