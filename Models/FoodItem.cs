namespace RestaurantOrderingSystem.Models
{
    public sealed class FoodItem : MenuItem
    {
        public bool IsVegetarian { get; set; }

        public FoodItem(string mealId, string mealName, MealCategory category, decimal price, bool isAvailable, bool isVegetarian)
            : base(mealId, mealName, category, price, isAvailable)
        {
            IsVegetarian = isVegetarian;
        }

        public override string DisplayLabel
        {
            get { return IsVegetarian ? MealName + " (V)" : MealName; }
        }
    }
}
