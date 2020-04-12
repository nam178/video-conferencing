using MediaServer.Common.Commands;
using MediaServer.Models;

namespace MediaServer.Core.Common
{
    public interface IRemoteDeviceRequestHandler<TCommandArgs, TResponse> 
        : ICommandHandlerWithResponse<IRemoteDevice, TCommandArgs, TResponse>
    {

    }
}
