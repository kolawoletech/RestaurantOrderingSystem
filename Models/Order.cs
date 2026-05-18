using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantOrderingSystem.Models
{
    public sealed class Order
    {
        private static int _nextNumber = 1000;

        private readonly List<OrderItem> _items = new List<OrderItem>();

        public int OrderNumber { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CashierUsername { get; private set; }

        public IReadOnlyList<OrderItem> Items { get { return _items.AsReadOnly(); } }

        public Order(string cashierUsername)
        {
            if (string.IsNullOrWhiteSpace(cashierUsername))
                throw new ArgumentException("Cashier username is required.", nameof(cashierUsername));

            OrderNumber = System.Threading.Interlocked.Increment(ref _nextNumber);
            CreatedAt = DateTime.Now;
            CashierUsername = cashierUsername;
        }

        public void AddItem(MenuItem item, int quantity)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (!item.IsAvailable)
                throw new InvalidOperationException(string.Format("'{0}' is not available.", item.MealName));

            var existing = _items.FirstOrDefault(i => i.Item.MealId == item.MealId);
            if (existing != null)
            {
                _items.Remove(existing);
                _items.Add(new OrderItem(item, existing.Quantity + quantity));
            }
            else
            {
                _items.Add(new OrderItem(item, quantity));
            }
        }

        public void RemoveItem(string mealId)
        {
            _items.RemoveAll(i => i.Item.MealId == mealId);
        }

        public void Clear() { _items.Clear(); }

        public decimal Subtotal { get { return _items.Sum(i => i.LineTotal); } }
        public decimal Tax { get { return Math.Round(Subtotal * 0.15m, 2); } } // 15% VAT
        public decimal Total { get { return Subtotal + Tax; } }
    }
}
