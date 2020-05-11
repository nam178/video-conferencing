using MediaServer.Api.WebSocket.CommandArgs;
using MediaServer.Api.WebSocket.Net;
using MediaServer.Common.Patterns;
using MediaServer.Core.Services.Negotiation.Handlers;
using System.Threading.Tasks;

namespace MediaServer.Api.WebSocket.CommandHandlers
{
    sealed class SetAnswerCommandHandler : IHandler<IWebSocketRemoteDevice, CommandArgs.SetAnswer>
    {
        readonly IAnswerHandler _answerHandler;

        public SetAnswerCommandHandler(IAnswerHandler answerHandler)
        {
            _answerHandler = answerHandler
                ?? throw new System.ArgumentNullException(nameof(answerHandler));
        }

        public Task HandleAsync(IWebSocketRemoteDevice device, SetAnswer args)
        {
            return _answerHandler.HandleAsync(device, args.PeerConnectionId, args.OfferId, args.Answer);
        }
    }
}
