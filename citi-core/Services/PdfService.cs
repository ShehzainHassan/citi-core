using citi_core.Dto;
using citi_core.Interfaces;
using QuestPDF.Fluent;
namespace citi_core.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateTransactionReportPdf(TransactionReportResponse report)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Transaction Report").FontSize(20).Bold();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Summary: Income: {report.Summary.TotalIncome:C}, Expenses: {report.Summary.TotalExpenses:C}, Net: {report.Summary.NetAmount:C}");
                        foreach (var group in report.GroupedTransactions)
                        {
                            col.Item().Text(group.Key).Bold();
                            foreach (var txn in group.Value)
                            {
                                col.Item().Text($"{txn.TransactionDate:yyyy-MM-dd} | {txn.Reference} | {txn.FormattedAmount} | {txn.Description}");
                            }
                        }
                    });
                });
            }).GeneratePdf();
        }

    }
}
