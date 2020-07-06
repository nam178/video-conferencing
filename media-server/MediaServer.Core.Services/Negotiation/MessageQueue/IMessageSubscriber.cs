using MediaServer.Common.Patterns;

namespace MediaServer.Core.Services.Negotiation.MessageQueue
{
    interface IMessageSubscriber
    {
        bool CanHandle(Message message);

        void Handle(Message message, Callback completionCallback);
    }
}
