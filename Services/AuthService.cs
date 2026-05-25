using System;
using RestaurantOrderingSystem.Data;
using RestaurantOrderingSystem.Exceptions;
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

        // LESSON: Lesson 11 — throwing a custom exception instead of
        // returning null. The PDF (§2.4.1) explains why specific exceptions
        // are better than sentinel return values: callers can't "forget"
        // to handle them — the compiler/runtime forces the issue.
        public Employee Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidLoginException(username, "Username is required.");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidLoginException(username, "Password is required.");

            var user = _users.FindByUsername(username);
            if (user == null || !user.VerifyPassword(password))
                throw new InvalidLoginException(username);

            return user;
        }
    }
}
