using MediaServer.Core.Models;
using System.Collections.Generic;

namespace MediaServer.Core.Models.Repositories
{
    /// <summary>
    /// Designed to have one user repository for each room
    /// </summary>
    public interface IUserProfileCollection : IEnumerable<User>
    {
        /// <summary>
        /// Add specified user into the repo
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If user already exist</exception>
        void AddUser(User user);

        /// <summary>
        /// Find the user by name
        /// </summary>
        User GetUserByName(string username);
    }
}
