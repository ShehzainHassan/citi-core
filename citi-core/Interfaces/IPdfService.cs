using citi_core.Dto;

namespace citi_core.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerateTransactionReportPdf(TransactionReportResponse report);
    }
}
