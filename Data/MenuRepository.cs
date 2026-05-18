using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantOrderingSystem.Interfaces;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Data
{
    // In-memory implementation. To migrate to SQL Server, build a SqlMenuRepository
    // that implements the same IRepository<MenuItem> contract and inject it instead.
    public sealed class MenuRepository : IRepository<MenuItem>
    {
        private readonly List<MenuItem> _items = new List<MenuItem>();

        public MenuRepository()
        {
            Seed();
        }

        private void Seed()
        {
            _items.Add(new FoodItem("F001", "Margherita Pizza", MealCategory.Main, 89.99m, true, true));
            _items.Add(new FoodItem("F002", "Beef Burger",      MealCategory.Main, 75.50m, true, false));
            _items.Add(new FoodItem("F003", "Caesar Salad",     MealCategory.Starter, 55.00m, true, true));
            _items.Add(new FoodItem("F004", "Chocolate Cake",   MealCategory.Dessert, 42.00m, true, true));
            _items.Add(new FoodItem("F005", "French Fries",     MealCategory.SideDish, 28.00m, true, true));
            _items.Add(new DrinkItem("D001", "Coca-Cola",        18.50m, true, 330));
            _items.Add(new DrinkItem("D002", "Sparkling Water",  22.00m, true, 500));
            _items.Add(new DrinkItem("D003", "Orange Juice",     30.00m, true, 250));
        }

        public IReadOnlyList<MenuItem> GetAll() { return _items.AsReadOnly(); }

        public MenuItem GetById(string id)
        {
            return _items.FirstOrDefault(i => string.Equals(i.MealId, id, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(MenuItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (GetById(entity.MealId) != null)
                throw new InvalidOperationException("A meal with that ID already exists.");
            _items.Add(entity);
        }

        public void Update(MenuItem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var existing = GetById(entity.MealId);
            if (existing == null)
                throw new InvalidOperationException("Meal not found.");
            _items.Remove(existing);
            _items.Add(entity);
        }

        public void Delete(string id)
        {
            var existing = GetById(id);
            if (existing == null)
                throw new InvalidOperationException("Meal not found.");
            _items.Remove(existing);
        }

        public IReadOnlyList<MenuItem> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return GetAll();
            var k = keyword.Trim();
            return _items
                .Where(i => i.MealName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0
                         || i.MealId.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList()
                .AsReadOnly();
        }
    }
}
