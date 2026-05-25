using System;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Exceptions
{
    // LESSON: Lesson 11 (§2.4.3) — Custom exception inheriting from Exception.
    //
    // Thrown by Order when a caller asks for an illegal state transition,
    // e.g. "MarkReady" on something that is still Pending. Carrying the
    // current and expected statuses lets the UI show a useful message
    // and lets future code react programmatically (e.g. log only one type).
    public sealed class InvalidOrderStateException : Exception
    {
        public OrderStatus Current { get; }
        public OrderStatus Expected { get; }
        public string Action { get; }

        public InvalidOrderStateException(OrderStatus current, OrderStatus expected, string action)
            : base(string.Format("Cannot {0}: order is {1}, expected {2}.", action, current, expected))
        {
            Current = current;
            Expected = expected;
            Action = action;
        }

        // Overload for "anything is wrong, no specific expected state"
        // (e.g. trying to cancel an already-completed order).
        // LESSON: Lesson 10 — overloaded constructors:
        //   same name (InvalidOrderStateException), different signature.
        public InvalidOrderStateException(string message) : base(message) { }
    }
}
