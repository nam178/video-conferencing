using MediaServer.Models;

namespace MediaServer.Core.Models
{
    interface IRemoteDeviceData
    {
        /// <summary>
        /// The room in which the device belongs to
        /// </summary>
        Room Room { get; }

        /// <summary>
        /// The user profile in which the device belongs to
        /// </summary>
        UserProfile User { get; }
    }
}
