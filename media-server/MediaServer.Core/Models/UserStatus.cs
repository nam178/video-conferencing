namespace MediaServer.Core.Models
{
    public enum UserStatus
    {
        /// <summary>
        /// The user is online. There are devices connected and they are sending heartbeats
        /// </summary>
        Online,

        /// <summary>
        /// The user is probably offline, devices connected
        /// but not sending heartbeats 
        /// </summary>
        Idle,

        /// <summary>
        /// The user is offline, i.e. no device connected
        /// </summary>
        Offline
    }
}