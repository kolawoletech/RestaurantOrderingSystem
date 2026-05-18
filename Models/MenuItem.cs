using System;
using RestaurantOrderingSystem.Interfaces;

namespace RestaurantOrderingSystem.Models
{
    // Abstract base for any item that can appear on the menu.
    // Implements IOrderable so it can be placed on an Order.
    public abstract class MenuItem : IOrderable
    {
        public string MealId { get; private set; }
        public string MealName { get; set; }
        public MealCategory Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        protected MenuItem(string mealId, string mealName, MealCategory category, decimal price, bool isAvailable)
        {
            if (string.IsNullOrWhiteSpace(mealId))
                throw new ArgumentException("Meal ID is required.", nameof(mealId));
            if (string.IsNullOrWhiteSpace(mealName))
                throw new ArgumentException("Meal name is required.", nameof(mealName));
            if (price < 0m)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            MealId = mealId.Trim();
            MealName = mealName.Trim();
            Category = category;
            Price = price;
            IsAvailable = isAvailable;
        }

        // Polymorphic — each concrete type can describe itself differently.
        public abstract string DisplayLabel { get; }

        // IOrderable
        public decimal GetLineTotal(int quantity)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
            return Price * quantity;
        }
    }
}
