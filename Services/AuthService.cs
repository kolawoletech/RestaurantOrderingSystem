using System;
using RestaurantOrderingSystem.Data;
using RestaurantOrderingSystem.Interfaces;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly UserRepository _users;

        public AuthService(UserRepository users)
        {
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public Employee Authenticate(string username, string password)
        {
            var user = _users.FindByUsername(username);
            if (user == null) return null;
            return user.VerifyPassword(password) ? user : null;
        }
    }
}
