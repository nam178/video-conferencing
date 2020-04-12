using MediaServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    sealed class RemoteDeviceCollection : IRemoteDeviceCollection
    {
        readonly List<IRemoteDevice> _remoteDevices = new List<IRemoteDevice>();

        public void Add(IRemoteDevice device)
        {
            if(device is null)
                throw new System.ArgumentNullException(nameof(device));
            if(_remoteDevices.Contains(device))
            {
                throw new InvalidOperationException($"Device already added");
            }
            _remoteDevices.Add(device);
        }

        public IEnumerator<IRemoteDevice> GetEnumerator() => _remoteDevices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
