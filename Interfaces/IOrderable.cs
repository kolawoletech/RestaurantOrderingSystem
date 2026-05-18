namespace RestaurantOrderingSystem.Interfaces
{
    // Anything that can be added to an Order implements this contract.
    // Lets us treat food, drinks, and any future product type uniformly.
    public interface IOrderable
    {
        string MealId { get; }
        string MealName { get; }
        decimal Price { get; }
        bool IsAvailable { get; }
        decimal GetLineTotal(int quantity);
    }
}
