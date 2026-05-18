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
        public IReadOnlyList<MenuItem> GetAvailable()
        {
            var all = _repo.GetAll();
            var list = new List<MenuItem>();
            foreach (var i in all) if (i.IsAvailable) list.Add(i);
            return list.AsReadOnly();
        }
        public IReadOnlyList<MenuItem> Search(string keyword) { return _repo.Search(keyword); }

        public void AddFood(string id, string name, MealCategory category, decimal price, bool available, bool vegetarian)
        {
            _repo.Add(new FoodItem(id, name, category, price, available, vegetarian));
        }

        public void AddDrink(string id, string name, decimal price, bool available, int volumeMl)
        {
            _repo.Add(new DrinkItem(id, name, price, available, volumeMl));
        }

        public void Update(MenuItem item) { _repo.Update(item); }
        public void Delete(string id)     { _repo.Delete(id); }
    }
}
