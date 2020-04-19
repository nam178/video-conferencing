using MediaServer.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MediaServer.Core.Repositories
{
    sealed class UserProfileCollection : IUserProfileCollection
    {
        readonly List<User> _users = new List<User>();

        public void AddUser(User user)
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

        public User GetUserByName(string username) => _users.FirstOrDefault(u => CompareUsername(username, u.Username));

        public IEnumerator<User> GetEnumerator() => _users.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _users.GetEnumerator();
    }
}
