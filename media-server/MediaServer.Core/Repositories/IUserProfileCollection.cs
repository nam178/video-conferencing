using MediaServer.Core.Models;
using System.Collections.Generic;

namespace MediaServer.Core.Repositories
{
    /// <summary>
    /// Designed to have one user repository for each room
    /// </summary>
    public interface IUserProfileCollection : IEnumerable<UserProfile>
    {
        /// <summary>
        /// Add specified user into the repo
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If user already exist</exception>
        void AddUser(UserProfile user);

        /// <summary>
        /// Find the user by name
        /// </summary>
        UserProfile GetUserByName(string username);
    }
}
