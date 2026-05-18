namespace RestaurantOrderingSystem.Models
{
    public sealed class DrinkItem : MenuItem
    {
        public int VolumeMl { get; set; }

        public DrinkItem(string mealId, string mealName, decimal price, bool isAvailable, int volumeMl)
            : base(mealId, mealName, MealCategory.Drink, price, isAvailable)
        {
            VolumeMl = volumeMl;
        }

        public override string DisplayLabel
        {
            get { return string.Format("{0} ({1}ml)", MealName, VolumeMl); }
        }
    }
}
