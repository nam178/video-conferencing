using MediaServer.Common.Patterns;

namespace MediaServer.Core.Services.Negotiation
{
    interface INegotiationMessageSubscriber
    {
        bool CanHandle(NegotiationMessage message);

        void Handle(NegotiationMessage message, Observer completionCallback);
    }
}
