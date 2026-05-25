using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantOrderingSystem.Exceptions;

namespace RestaurantOrderingSystem.Models
{
    // LESSON: Lesson 11 — Exceptions (throwing for invalid state).
    //
    // Each lifecycle transition and each item edit VALIDATES first and
    // THROWS InvalidOperationException with a clear message if the request
    // makes no sense (e.g. "complete" an order that was never sent to the
    // kitchen, or add items to one that's already Preparing). Forms wrap
    // these calls in try/catch and show the message — the classic L11
    // pattern of "model throws, UI handles."
    //
    // Two responsibilities live here:
    //   1. Lifecycle (Pending -> Preparing -> Ready -> Completed, with
    //      Cancel as an escape hatch). See MarkXxx / Cancel.
    //   2. Reserving stock when items are added; returning stock when
    //      they are removed or when a still-Pending order is cancelled.
    //
    // Both are enforced HERE, not in the UI. A future Kitchen form, REST
    // API, or unit test all get the same guarantees for free.
    public sealed class Order
    {
        private static int _nextNumber = 1000;

        private readonly List<OrderItem> _items = new List<OrderItem>();

        public int OrderNumber { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CashierUsername { get; private set; }

        public OrderStatus Status { get; private set; }
        public DateTime? SubmittedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        public IReadOnlyList<OrderItem> Items { get { return _items.AsReadOnly(); } }

        public Order(string cashierUsername)
        {
            if (string.IsNullOrWhiteSpace(cashierUsername))
                throw new ArgumentException("Cashier username is required.", nameof(cashierUsername));

            OrderNumber = System.Threading.Interlocked.Increment(ref _nextNumber);
            CreatedAt = DateTime.Now;
            CashierUsername = cashierUsername;
            Status = OrderStatus.Pending;
        }

        public void AddItem(MenuItem item, int quantity)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity));
            RequireEditable();

            // Check IsOrderable (manual availability AND in-stock),
            // not just IsAvailable. Each branch below throws a specific
            // message — the L11 §2.4.1 idea of "specific exceptions first".
            if (!item.IsAvailable)
                throw new InvalidOperationException(string.Format("'{0}' is not available.", item.MealName));
            // L11: specific custom exception. Lets a form do
            //   catch (OutOfStockException ex) { ...show stock-specific UI... }
            //   catch (Exception ex)           { ...generic fallback...      }
            if (item.StockQuantity < quantity)
                throw new OutOfStockException(item, quantity);

            // Reserve the stock NOW. If anything below throws we've already
            // decremented — but stock is only decremented once we know the
            // numbers are valid, so this is safe.
            item.UpdateStock(-quantity);

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
            RequireEditable();
            var line = _items.FirstOrDefault(i => i.Item.MealId == mealId);
            if (line == null) return;
            line.Item.UpdateStock(+line.Quantity); // return reserved stock
            _items.Remove(line);
        }

        public void Clear()
        {
            RequireEditable();
            ReturnAllStock();
            _items.Clear();
        }

        public decimal Subtotal { get { return _items.Sum(i => i.LineTotal); } }
        public decimal Tax { get { return Math.Round(Subtotal * 0.15m, 2); } } // 15% VAT
        public decimal Total { get { return Subtotal + Tax; } }

        // ----- Lifecycle transitions ---------------------------------------
        internal void MarkSubmittedForKitchen()
        {
            RequireStatus(OrderStatus.Pending, "submit");
            if (_items.Count == 0)
                throw new InvalidOperationException("Cannot submit an empty order.");
            Status = OrderStatus.Preparing;
            SubmittedAt = DateTime.Now;
        }

        public void MarkReady()
        {
            RequireStatus(OrderStatus.Preparing, "mark as ready");
            Status = OrderStatus.Ready;
        }

        public void MarkCompleted()
        {
            RequireStatus(OrderStatus.Ready, "complete");
            Status = OrderStatus.Completed;
            CompletedAt = DateTime.Now;
        }

        // LESSON: state-dependent rollback.
        // If we cancel a Pending order, the ingredients were never used, so
        // we put the stock back. If we cancel something already in the
        // kitchen, the food has been prepared and the stock is gone — we
        // change status but DON'T refund stock. This is exactly the kind
        // of business rule the model layer should enforce.
        public void Cancel()
        {
            if (Status == OrderStatus.Completed)
                throw new InvalidOrderStateException("A completed order cannot be cancelled.");
            if (Status == OrderStatus.Cancelled)
                throw new InvalidOrderStateException("Order is already cancelled.");

            bool refundStock = (Status == OrderStatus.Pending);
            Status = OrderStatus.Cancelled;
            if (refundStock) ReturnAllStock();
        }

        // ----- Helpers -----------------------------------------------------
        private void ReturnAllStock()
        {
            foreach (var line in _items)
                line.Item.UpdateStock(+line.Quantity);
        }

        private void RequireStatus(OrderStatus expected, string action)
        {
            if (Status != expected)
                throw new InvalidOrderStateException(Status, expected, action);
        }

        private void RequireEditable()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOrderStateException(
                    string.Format("Order #{0} is {1} and can no longer be edited.", OrderNumber, Status));
        }
    }
}
