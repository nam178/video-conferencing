using System;
using System.Collections;
using System.Collections.Generic;

namespace MediaServer.Core.Models.Repositories
{
    sealed class RemoteDeviceCollection : IRemoteDeviceCollection
    {
        readonly List<IRemoteDevice> _data = new List<IRemoteDevice>();

        public void Add(IRemoteDevice device)
        {
            if(device is null)
                throw new ArgumentNullException(nameof(device));
            if(_data.Contains(device))
            {
                throw new InvalidOperationException($"Device already added");
            }
            _data.Add(device);
        }

        public IEnumerator<IRemoteDevice> GetEnumerator() => _data.GetEnumerator();

        public void Remove(IRemoteDevice device)
        {
            if(_data.Contains(device))
            {
                _data.Remove(device);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
