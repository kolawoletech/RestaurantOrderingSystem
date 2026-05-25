// LESSON: OOP foundations (covered in earlier lessons) — Enums.
//
// Not in Week 4's L10/L11/L12, but used here as supporting infrastructure
// for the Lesson 11 exception examples in Order.cs.
//
// An enum gives a fixed, named set of values. We use one here instead of a
// string ("pending", "ready"...) because:
//   1. The compiler catches typos.
//   2. IntelliSense lists every legal value.
//   3. switch statements stay exhaustive and readable.
//
// The order of the values mirrors the natural lifecycle of an order.
namespace RestaurantOrderingSystem.Models
{
    public enum OrderStatus
    {
        Pending = 0,     // built by the cashier, not yet sent to the kitchen
        Preparing = 1,   // kitchen has accepted it
        Ready = 2,       // food is plated, waiting for pickup / serving
        Completed = 3,   // handed to customer, money taken
        Cancelled = 4    // terminal: voided before completion
    }
}
