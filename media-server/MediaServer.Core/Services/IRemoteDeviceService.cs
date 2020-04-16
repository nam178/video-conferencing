using MediaServer.Common.Mediator;
using MediaServer.Models;

namespace MediaServer.Core.Services
{
    /// <summary>
    /// Represents a service that answer commands from a remote device.
    /// </summary>
    /// <typeparam name="TCommandArgs"></typeparam>
    public interface IRemoteDeviceService<TCommandArgs> : IHandler<IRemoteDevice, TCommandArgs>
    {

    }

    /// <summary>
    /// Represents a service that answer commands from a remote device,
    /// and return a result to that device.
    /// </summary>
    public interface ICoreService<TCommandArgs, TResponse> : IMapper<IRemoteDevice, TCommandArgs, TResponse>
    {

    }
}
