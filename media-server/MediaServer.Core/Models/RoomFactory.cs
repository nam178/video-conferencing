using Autofac;
using MediaServer.Core.Adapters;
using MediaServer.Core.Services.Negotiation;
using MediaServer.Models;
using System;
using System.Collections.Generic;

namespace MediaServer.Core.Models
{
    sealed class RoomFactory : IRoomFactory
    {
        readonly ILifetimeScope _lifetimeScope;

        public RoomFactory(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
        }

        public IRoom Create(RoomId id) => new Room(
            id, 
            _lifetimeScope.Resolve<WebRtcInfraAdapter>(),
            _lifetimeScope.Resolve<IEnumerable<INegotiationMessageSubscriber>>());
    }
}
