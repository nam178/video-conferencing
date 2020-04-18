using Autofac;
using MediaServer.Models;
using System;

namespace MediaServer.Core.Models
{
    sealed class RoomFactory : IRoomFactory
    {
        readonly ILifetimeScope _lifetimeScope;

        public RoomFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public IRoom Create() => new Room(_lifetimeScope.Resolve<IPeerConnectionFactory>());
    }
}
