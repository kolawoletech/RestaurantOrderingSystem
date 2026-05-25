using System;

namespace RestaurantOrderingSystem.Exceptions
{
    // LESSON: Lesson 11 (§2.4.3) + Lesson 10 (overloaded constructors).
    //
    // Three overloaded constructors so callers can pick the level of
    // detail they want — exactly the §2.3.1 pattern from the PDF, applied
    // to a constructor instead of an ordinary method.
    public sealed class InvalidLoginException : Exception
    {
        public string AttemptedUsername { get; }

        public InvalidLoginException()
            : base("Invalid username or password.") { }

        public InvalidLoginException(string attemptedUsername)
            : base("Invalid username or password.")
        {
            AttemptedUsername = attemptedUsername;
        }

        public InvalidLoginException(string attemptedUsername, string message)
            : base(message)
        {
            AttemptedUsername = attemptedUsername;
        }
    }
}
