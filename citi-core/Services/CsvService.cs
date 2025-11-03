using citi_core.Dto;
using citi_core.Interfaces;
using System.Text;

namespace citi_core.Services
{
    public class CsvService : ICsvService
    {
        public string GenerateTransactionReportCsv(TransactionReportResponse report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Date,Reference,Type,Amount,Description,Category,Status");

            foreach (var group in report.GroupedTransactions)
            {
                foreach (var txn in group.Value)
                {
                    sb.AppendLine($"{txn.TransactionDate:yyyy-MM-dd},{txn.Reference},{txn.Type},{txn.Amount},{Escape(txn.Description)},{txn.Category?.Name},{txn.Status}");
                }
            }

            return sb.ToString();
        }

        private string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

    }
}
