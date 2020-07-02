using System.Collections.Generic;

namespace MediaServer.Core.Models.Repositories
{
    public interface IRemoteDeviceCollection : IEnumerable<IRemoteDevice>
    {
        void Add(IRemoteDevice device);

        void Remove(IRemoteDevice device);
    }
}
