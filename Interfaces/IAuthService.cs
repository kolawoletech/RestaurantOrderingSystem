using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Interfaces
{
    public interface IAuthService
    {
        // Returns the authenticated Employee on success.
        // Throws InvalidLoginException on bad credentials or empty inputs.
        Employee Authenticate(string username, string password);
    }
}
