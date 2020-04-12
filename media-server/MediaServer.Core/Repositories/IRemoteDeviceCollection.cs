using MediaServer.Models;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    public interface IRemoteDeviceCollection : IEnumerable<IRemoteDevice>
    {
        void Add(IRemoteDevice device);
    }
}
