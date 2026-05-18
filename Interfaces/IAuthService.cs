using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Interfaces
{
    public interface IAuthService
    {
        // Returns the authenticated Employee on success, or null on failure.
        Employee Authenticate(string username, string password);
    }
}
