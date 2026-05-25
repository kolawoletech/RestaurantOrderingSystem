using System;
using System.Collections.Generic;
using RestaurantOrderingSystem.Data;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Services
{
    // Thin business-logic layer in front of the repository.
    // Forms talk to this class, never directly to the repo.
    public sealed class MenuService
    {
        private readonly MenuRepository _repo;

        public MenuService(MenuRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public IReadOnlyList<MenuItem> GetAll() { return _repo.GetAll(); }
        // "Available" now means BOTH the admin flag and stock > 0.
        // Cashier-facing screens use this so out-of-stock items disappear.
        public IReadOnlyList<MenuItem> GetAvailable()
        {
            var all = _repo.GetAll();
            var list = new List<MenuItem>();
            foreach (var i in all) if (i.IsOrderable) list.Add(i);
            return list.AsReadOnly();
        }

        // Restock — admin tops up an item.
        public void Restock(string mealId, int quantity)
        {
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity), "Restock must be positive.");
            var item = _repo.GetById(mealId);
            if (item == null) throw new InvalidOperationException("Meal not found.");
            item.UpdateStock(+quantity);
        }

        public IReadOnlyList<MenuItem> GetLowStock()
        {
            var all = _repo.GetAll();
            var list = new List<MenuItem>();
            foreach (var i in all) if (i.IsLowStock) list.Add(i);
            return list.AsReadOnly();
        }
        public IReadOnlyList<MenuItem> Search(string keyword) { return _repo.Search(keyword); }

        // LESSON: Lesson 10 (§2.3.1) — Method overloading.
        //
        // Two real overloads (same name, different signatures) — NOT default
        // arguments. The compiler picks which version to call based on
        // the arguments at the call site, exactly as the PDF describes:
        //
        //   MenuService.AddFood(id, name, cat, price, available, veg)
        //       -> the short overload, stock defaults to 0
        //
        //   MenuService.AddFood(id, name, cat, price, available, veg, stock, threshold)
        //       -> the long overload, full stock control
        //
        // The short overload delegates to the long one — a common pattern
        // that avoids duplicating the actual work.

        public void AddFood(string id, string name, MealCategory category, decimal price, bool available, bool vegetarian)
        {
            AddFood(id, name, category, price, available, vegetarian, 0, 5);
        }

        public void AddFood(string id, string name, MealCategory category, decimal price, bool available, bool vegetarian,
                            int stockQuantity, int lowStockThreshold)
        {
            _repo.Add(new FoodItem(id, name, category, price, available, vegetarian, stockQuantity, lowStockThreshold));
        }

        public void AddDrink(string id, string name, decimal price, bool available, int volumeMl)
        {
            AddDrink(id, name, price, available, volumeMl, 0, 5);
        }

        public void AddDrink(string id, string name, decimal price, bool available, int volumeMl,
                             int stockQuantity, int lowStockThreshold)
        {
            _repo.Add(new DrinkItem(id, name, price, available, volumeMl, stockQuantity, lowStockThreshold));
        }

        public void Update(MenuItem item) { _repo.Update(item); }
        public void Delete(string id) { _repo.Delete(id); }
    }
}
