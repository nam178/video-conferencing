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
    }
}
