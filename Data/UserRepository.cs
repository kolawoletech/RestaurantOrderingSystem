using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Data
{
    public sealed class UserRepository
    {
        private readonly List<Employee> _employees = new List<Employee>();

        public UserRepository()
        {
            // Seeded demo accounts. In a real system these would come from a
            // secured database with hashed passwords.
            _employees.Add(new Admin("E001",   "Alice Admin",   "admin",   "admin123"));
            _employees.Add(new Cashier("E002", "Charlie Cashier", "cashier", "cashier123"));
        }

        public Employee FindByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return _employees.FirstOrDefault(
                e => string.Equals(e.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<Employee> GetAll() { return _employees.AsReadOnly(); }
    }
}
