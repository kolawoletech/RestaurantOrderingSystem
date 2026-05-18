using System.Collections.Generic;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Data
{
    // Stores completed orders in memory. Trivially swappable for SQL.
    public sealed class OrderRepository
    {
        private readonly List<Order> _orders = new List<Order>();

        public void Save(Order order) { _orders.Add(order); }

        public IReadOnlyList<Order> GetAll() { return _orders.AsReadOnly(); }
    }
}
