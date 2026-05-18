using System.Globalization;
using System.Text;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Utilities
{
    public static class ReceiptFormatter
    {
        private static readonly CultureInfo Money = CultureInfo.GetCultureInfo("en-ZA");

        public static string Format(Order order)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================");
            sb.AppendLine("           RESTAURANT ORDERING SYSTEM           ");
            sb.AppendLine("================================================");
            sb.AppendLine("Order #:  " + order.OrderNumber);
            sb.AppendLine("Date:     " + order.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine("Cashier:  " + order.CashierUsername);
            sb.AppendLine("------------------------------------------------");
            sb.AppendLine(string.Format("{0,-22}{1,4}{2,10}{3,12}", "Item", "Qty", "Price", "Total"));
            sb.AppendLine("------------------------------------------------");

            foreach (var line in order.Items)
            {
                sb.AppendLine(string.Format(
                    "{0,-22}{1,4}{2,10}{3,12}",
                    Trim(line.Item.MealName, 22),
                    line.Quantity,
                    line.Item.Price.ToString("C", Money),
                    line.LineTotal.ToString("C", Money)));
            }

            sb.AppendLine("------------------------------------------------");
            sb.AppendLine(string.Format("{0,40}{1,8}", "Subtotal:", order.Subtotal.ToString("C", Money)));
            sb.AppendLine(string.Format("{0,40}{1,8}", "VAT (15%):", order.Tax.ToString("C", Money)));
            sb.AppendLine(string.Format("{0,40}{1,8}", "TOTAL:", order.Total.ToString("C", Money)));
            sb.AppendLine("================================================");
            sb.AppendLine("            Thank you for dining with us!       ");
            sb.AppendLine("================================================");
            return sb.ToString();
        }

        private static string Trim(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= max ? value : value.Substring(0, max - 1) + "…";
        }
    }
}
