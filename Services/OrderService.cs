using System;
using RestaurantOrderingSystem.Data;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Services
{
    public sealed class OrderService
    {
        private readonly OrderRepository _repo;

        public OrderService(OrderRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Order CreateOrder(string cashierUsername)
        {
            return new Order(cashierUsername);
        }

        public void Submit(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (order.Items.Count == 0)
                throw new InvalidOperationException("Cannot submit an empty order.");
            _repo.Save(order);
        }
    }
}
