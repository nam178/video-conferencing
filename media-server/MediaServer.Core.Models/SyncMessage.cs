using System;

namespace MediaServer.Models
{
    /// <summary>
    /// The message that we will send to devices at an interval
    /// or when devices join/left the room, so devices can update their UIs.
    /// </summary>
    public class SyncMessage
    {
        public UserInfo[] Users { get; set; }

        public class UserInfo
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public DeviceInfo[] Devices { get; set; }
        }

        public class DeviceInfo
        {
            public Guid DeviceId { get; set; }
        }
    }
}