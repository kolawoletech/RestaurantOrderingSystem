using System;

namespace RestaurantOrderingSystem.Models
{
    // Represents a single line on an Order: one menu item + a quantity.
    public sealed class OrderItem
    {
        public MenuItem Item { get; private set; }
        public int Quantity { get; private set; }

        public OrderItem(MenuItem item, int quantity)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
            Item = item;
            Quantity = quantity;
        }

        public decimal LineTotal { get { return Item.GetLineTotal(Quantity); } }
    }
}
