using System;

namespace MediaServer.Models
{
    public class RemoteDeviceUserUpdateMessage
    {
        public UserProfile[] Users { get; set; }

        public class UserProfile
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public bool IsOnline { get; set; }
        }
    }
}