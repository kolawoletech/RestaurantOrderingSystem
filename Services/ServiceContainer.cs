using RestaurantOrderingSystem.Data;
using RestaurantOrderingSystem.Interfaces;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Services
{
    // Composition root: a single place where repositories and services are built
    // and wired together. Forms receive a ServiceContainer rather than newing up
    // their own dependencies — this is poor-man's dependency injection.
    public sealed class ServiceContainer
    {
        public UserRepository Users { get; }
        public MenuRepository Menu { get; }
        public OrderRepository Orders { get; }

        public IAuthService Auth { get; }
        public MenuService MenuService { get; }
        public OrderService OrderService { get; }

        // Set after a successful login. Forms read this to know who is signed in.
        public Employee CurrentUser { get; set; }

        public ServiceContainer()
        {
            Users = new UserRepository();
            Menu = new MenuRepository();
            Orders = new OrderRepository();

            Auth = new AuthService(Users);
            MenuService = new MenuService(Menu);
            OrderService = new OrderService(Orders);
        }
    }
}
