using System;

namespace RestaurantOrderingSystem.Utilities
{
    // Centralised validation helpers. Keeps form code-behind clean.
    public static class Validator
    {
        public static void RequireText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(fieldName + " is required.");
        }

        public static decimal RequirePositiveDecimal(string raw, string fieldName)
        {
            decimal result;
            if (!decimal.TryParse(raw, out result))
                throw new ArgumentException(fieldName + " must be a number.");
            if (result < 0m)
                throw new ArgumentException(fieldName + " cannot be negative.");
            return result;
        }
    }
}
