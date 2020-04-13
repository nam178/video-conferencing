using MediaServer.Common.Commands;
using MediaServer.Models;
using MediaServer.Signalling.CommandArgs;
using MediaServer.Signalling.Net;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaServer.Signalling.Handlers
{
    sealed class SetOfferCommandHandler : ICommandHandler<WebSocketClient, SetOffer>
    {
        readonly Room _room;
        readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public SetOfferCommandHandler(Room room)
        {
            _room = room ?? throw new ArgumentNullException(nameof(room));
        }

        public async Task HandleAsync(WebSocketClient arg1, SetOffer arg2)
        {
            await _room.DispatchQueue.ExecuteAsync(delegate
            {
                // settings Remote SDP should never throw exception
                // no need to handle here
                _room.Peers.First().RemoteRtcSessionDescription = arg2.Offer;
            });
        }
    }
}
