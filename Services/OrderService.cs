using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantOrderingSystem.Data;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Services
{
    // LESSON: OOP foundations — Service layer (covered in earlier lessons).
    // Relevant to Lesson 11 because Submit/MarkReady/MarkCompleted/Cancel
    // can each THROW (the underlying Order enforces state rules) and the
    // calling form is expected to catch them.
    //
    // Forms call OrderService; OrderService talks to the Order object and the
    // repository. This means the UI never needs to know HOW an order changes
    // state — it just asks the service. When we add persistence later, only
    // this class changes; the forms stay the same. That's the value of layers.
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

        // Saves a Pending order and moves it into Preparing.
        public void Submit(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            order.MarkSubmittedForKitchen(); // throws if empty or wrong state
            _repo.Save(order);
        }

        // Kitchen workflow ---------------------------------------------------
        public void MarkReady(int orderNumber) { Find(orderNumber).MarkReady(); }
        public void MarkCompleted(int orderNumber) { Find(orderNumber).MarkCompleted(); }
        public void Cancel(int orderNumber) { Find(orderNumber).Cancel(); }

        // Queries used by future Kitchen / Reporting screens.
        public IReadOnlyList<Order> GetAll() { return _repo.GetAll(); }

        public IReadOnlyList<Order> GetByStatus(OrderStatus status)
        {
            return _repo.GetAll().Where(o => o.Status == status).ToList().AsReadOnly();
        }

        private Order Find(int orderNumber)
        {
            var order = _repo.GetById(orderNumber);
            if (order == null)
                throw new InvalidOperationException("Order #" + orderNumber + " was not found.");
            return order;
        }
    }
}
