namespace citi_core.Utilities
{
    public static class DateGroup
    {
        public static string GetDateGroup(DateTime date)
        {
            var today = DateTime.UtcNow.Date;
            var transactionDate = date.Date;

            if (transactionDate == today)
                return "Today";
            if (transactionDate == today.AddDays(-1))
                return "Yesterday";

            return transactionDate.ToString("MMM dd, yyyy");
        }

    }
}
