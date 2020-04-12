using MediaServer.Common.Commands;
using MediaServer.Models;
using NLog;
using System;
using System.Threading.Tasks;

namespace MediaServer.WebSocket.CommandHandlers
{
    sealed class Register { }

    sealed class RegisterCommandHandler : ICommandHandler<WebSocketClient, Register>
    {
        readonly Room _room;
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public RegisterCommandHandler(Room room)
        {
            _room = room ?? throw new System.ArgumentNullException(nameof(room));
        }

        public async Task HandleAsync(WebSocketClient arg1, Register arg2)
        {
            await _room.DispatchQueue.ExecuteAsync(async delegate
            {
                await _room.WaitForInitialisationAsync();

                try
                {
                    var peer = new Peer(arg1.ToString(), new WebSocketSignaller(arg1));

                    _room.AddPeer(peer);
                }
                catch(Exception ex)
                {
                    _logger.Fatal(ex);
                    throw new NotImplementedException($"TODO: what if AddPeer() fails?");
                }
            });
        }
    }
}
