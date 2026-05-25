using System.Collections.Generic;
using System.Linq;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Data
{
    // LESSON: OOP foundations — Repository pattern (covered in earlier lessons).
    //
    // The repository hides WHERE the data lives. Today it's a List in memory;
    // tomorrow it could be SQLite or SQL Server. As long as the public methods
    // stay the same, the rest of the program does not need to change.
    public sealed class OrderRepository
    {
        private readonly List<Order> _orders = new List<Order>();

        public void Save(Order order) { _orders.Add(order); }

        public IReadOnlyList<Order> GetAll() { return _orders.AsReadOnly(); }

        public Order GetById(int orderNumber)
        {
            return _orders.FirstOrDefault(o => o.OrderNumber == orderNumber);
        }
    }
}
