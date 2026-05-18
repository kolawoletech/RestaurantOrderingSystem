using System;

namespace RestaurantOrderingSystem.Models
{
    // Abstract base class — cannot be instantiated directly.
    // Demonstrates: abstraction, encapsulation, constructors, virtual methods.
    public abstract class Person
    {
        public string FullName { get; private set; }
        public string Username { get; private set; }

        protected Person(string fullName, string username)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.", nameof(username));

            FullName = fullName.Trim();
            Username = username.Trim();
        }

        // Polymorphic hook — each subclass describes itself.
        public abstract string RoleName { get; }

        public virtual string Describe()
        {
            return string.Format("{0} ({1}) — {2}", FullName, Username, RoleName);
        }
    }
}
