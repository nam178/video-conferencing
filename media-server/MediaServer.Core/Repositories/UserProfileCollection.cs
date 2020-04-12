using MediaServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Repositories
{
    sealed class UserProfileCollection : IUserProfileCollection
    {
        readonly List<UserProfile> _users = new List<UserProfile>();

        public void AddUser(UserProfile user)
        {
            if(user is null)
                throw new ArgumentNullException(nameof(user));
            if(_users.Any(u => CompareUsername(user.Username, u.Username)))
                throw new InvalidOperationException($"User {user.Username} already exist");
            _users.Add(user);
        }

        static bool CompareUsername(string username1, string username2)
        {
            return string.Equals(username1, username2, StringComparison.InvariantCulture);
        }

        public UserProfile GetUserByName(string username) => _users.FirstOrDefault(u => CompareUsername(username, u.Username));
    }
}
