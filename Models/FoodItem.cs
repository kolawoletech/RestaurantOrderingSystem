namespace RestaurantOrderingSystem.Models
{
    public sealed class FoodItem : MenuItem
    {
        public bool IsVegetarian { get; set; }

        public FoodItem(string mealId, string mealName, MealCategory category, decimal price, bool isAvailable,
                        bool isVegetarian, int stockQuantity = 0, int lowStockThreshold = 5)
            : base(mealId, mealName, category, price, isAvailable, stockQuantity, lowStockThreshold)
        {
            IsVegetarian = isVegetarian;
        }

        public override string DisplayLabel
        {
            get { return IsVegetarian ? MealName + " (V)" : MealName; }
        }
    }
}
