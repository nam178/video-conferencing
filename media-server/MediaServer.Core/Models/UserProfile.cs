using MediaServer.Core.Repositories;
using MediaServer.Models;
using System;

namespace MediaServer.Core.Models
{
    public sealed class UserProfile
    {
        /// <summary>
        /// This user's id in context of the room
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// This user's name in context of the room
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The devices associated with this user
        /// </summary>
        public IRemoteDeviceCollection Devices { get; } = new RemoteDeviceCollection();

        /// <summary>
        /// The room in which this user belongs to
        /// </summary>
        public IRoom Room { get; }

        public UserProfile(IRoom room)
        {
            Room = room ?? throw new ArgumentNullException(nameof(room));
        }

        public override string ToString() => $"[UserProfile {Username}, Id={Id}]";
    }
}
