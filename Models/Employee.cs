using System;

namespace RestaurantOrderingSystem.Models
{
    // Intermediate abstract class — every employee has a password and an ID,
    // but only concrete roles (Admin, Cashier) can be instantiated.
    public abstract class Employee : Person
    {
        public string EmployeeId { get; private set; }

        // Stored as plain text for teaching purposes only.
        // In a real system, store a salted hash (e.g. PBKDF2 / BCrypt).
        public string Password { get; private set; }

        protected Employee(string employeeId, string fullName, string username, string password)
            : base(fullName, username)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Employee ID is required.", nameof(employeeId));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            EmployeeId = employeeId.Trim();
            Password = password;
        }

        public bool VerifyPassword(string attempt)
        {
            return string.Equals(Password, attempt, StringComparison.Ordinal);
        }
    }
}
