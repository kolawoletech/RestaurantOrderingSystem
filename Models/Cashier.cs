namespace RestaurantOrderingSystem.Models
{
    public sealed class Cashier : Employee
    {
        public Cashier(string employeeId, string fullName, string username, string password)
            : base(employeeId, fullName, username, password) { }

        public override string RoleName { get { return "Cashier"; } }
    }
}
