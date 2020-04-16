using MediaServer.Common.Mediator;
using MediaServer.Models;

namespace MediaServer.Core.Services
{
    public interface ICoreService<TCommandArgs>
        : IHandler<IRemoteDevice, TCommandArgs>
    {

    }

    public interface ICoreService<TCommandArgs, TResponse>
        : IMapper<IRemoteDevice, TCommandArgs, TResponse>
    {

    }
}
