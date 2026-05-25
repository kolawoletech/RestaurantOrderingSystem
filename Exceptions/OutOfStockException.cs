using System;
using RestaurantOrderingSystem.Models;

// LESSON: Lesson 12 (§2.5) — Namespaces.
// All custom exceptions live in their own namespace,
// RestaurantOrderingSystem.Exceptions, so callers `using` it explicitly
// when they care. This mirrors the .NET BCL's own approach
// (System.IO.IOException, System.Net.WebException, etc.).
namespace RestaurantOrderingSystem.Exceptions
{
    // LESSON: Lesson 11 (§2.4.3) — Designing your own exception.
    //
    // SillyException in the PDF inherited from Exception and called the
    // base constructor with a message. We do the same here, then carry
    // extra structured data (the menu item and the requested quantity)
    // so calling code can render a precise message without parsing strings.
    public sealed class OutOfStockException : Exception
    {
        public string MealId { get; }
        public string MealName { get; }
        public int Available { get; }
        public int Requested { get; }

        public OutOfStockException(MenuItem item, int requested)
            : base(BuildMessage(item, requested))
        {
            MealId = item?.MealId ?? string.Empty;
            MealName = item?.MealName ?? string.Empty;
            Available = item?.StockQuantity ?? 0;
            Requested = requested;
        }

        private static string BuildMessage(MenuItem item, int requested)
        {
            if (item == null) return "Item is out of stock.";
            return string.Format("Not enough stock for '{0}'. Have {1}, requested {2}.",
                item.MealName, item.StockQuantity, requested);
        }
    }
}
