using System;
using RestaurantOrderingSystem.Exceptions;
using RestaurantOrderingSystem.Interfaces;

namespace RestaurantOrderingSystem.Models
{
    // LESSON: OOP foundations (encapsulation) + Lesson 11 (exceptions).
    //
    // The encapsulation idea — private setter + a controlled method — is
    // earlier-lesson material. The bit that fits Week 4 is UpdateStock:
    // when the requested change would break the invariant, it THROWS
    // InvalidOperationException with a useful Message. That's exactly the
    // Lesson 11 (§2.4) pattern: detect invalid input, raise a meaningful
    // exception, let the caller's try/catch decide how to surface it.
    //
    // Stock is exposed READ-ONLY. The only way to change it from outside is
    // through UpdateStock(delta), which refuses to drop below zero. This is
    // the core promise of encapsulation: the object protects its own data
    // from being put into an impossible state.
    //
    // We deliberately do NOT replace IsAvailable. Instead we add a second
    // computed flag, IsOrderable, that combines:
    //     IsAvailable      — admin can manually turn an item off (e.g. recipe
    //                        change, supplier issue)
    //     StockQuantity>0  — purely an inventory fact
    // Showing both teaches students the difference between a business rule
    // and a data fact — they often get conflated.
    public abstract class MenuItem : IOrderable
    {
        public string MealId { get; private set; }
        public string MealName { get; set; }
        public MealCategory Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }

        // Week 8 -----------------------------------------------------------
        public int StockQuantity { get; private set; }
        public int LowStockThreshold { get; set; }

        public bool IsInStock { get { return StockQuantity > 0; } }
        public bool IsLowStock { get { return StockQuantity <= LowStockThreshold; } }
        public bool IsOrderable { get { return IsAvailable && IsInStock; } }
        // ------------------------------------------------------------------

        protected MenuItem(string mealId, string mealName, MealCategory category, decimal price, bool isAvailable,
                           int stockQuantity = 0, int lowStockThreshold = 5)
        {
            if (string.IsNullOrWhiteSpace(mealId))
                throw new ArgumentException("Meal ID is required.", nameof(mealId));
            if (string.IsNullOrWhiteSpace(mealName))
                throw new ArgumentException("Meal name is required.", nameof(mealName));
            if (price < 0m)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
            if (stockQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(stockQuantity), "Stock cannot be negative.");
            if (lowStockThreshold < 0)
                throw new ArgumentOutOfRangeException(nameof(lowStockThreshold), "Threshold cannot be negative.");

            MealId = mealId.Trim();
            MealName = mealName.Trim();
            Category = category;
            Price = price;
            IsAvailable = isAvailable;
            StockQuantity = stockQuantity;
            LowStockThreshold = lowStockThreshold;
        }

        // Polymorphic — each concrete type can describe itself differently.
        public abstract string DisplayLabel { get; }

        // Adjust stock by a delta. Positive = restock; negative = consume.
        // Throws if a negative delta would push stock below zero — the
        // invariant the class protects.
        public void UpdateStock(int delta)
        {
            int next = StockQuantity + delta;
            // LESSON: Lesson 11 — throw a SPECIFIC custom exception.
            // Callers can catch OutOfStockException before a general
            // catch (Exception), matching the PDF §2.4.1 ordering rule.
            if (next < 0)
                throw new OutOfStockException(this, -delta);
            StockQuantity = next;
        }

        // IOrderable
        public decimal GetLineTotal(int quantity)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
            return Price * quantity;
        }
    }
}
