using MediaServer.Common.Mediator;
using MediaServer.Models;

namespace MediaServer.Core.Services
{
    public interface IRemoteDeviceRequestHandler<TCommandArgs>
        : IHandler<IRemoteDevice, TCommandArgs>
    {

    }

    public interface IRemoteDeviceRequestHandler<TCommandArgs, TResponse>
        : IMapper<IRemoteDevice, TCommandArgs, TResponse>
    {

    }
}
