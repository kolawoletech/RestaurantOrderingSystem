using System.Collections.Generic;

namespace RestaurantOrderingSystem.Interfaces
{
    // Generic CRUD contract. Today implemented by in-memory lists;
    // tomorrow swap in a SQL implementation without changing callers.
    public interface IRepository<T>
    {
        IReadOnlyList<T> GetAll();
        T GetById(string id);
        void Add(T entity);
        void Update(T entity);
        void Delete(string id);
    }
}
