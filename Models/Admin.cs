namespace RestaurantOrderingSystem.Models
{
    public sealed class Admin : Employee
    {
        public Admin(string employeeId, string fullName, string username, string password)
            : base(employeeId, fullName, username, password) { }

        public override string RoleName { get { return "Administrator"; } }
    }
}
