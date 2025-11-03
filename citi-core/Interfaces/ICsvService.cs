using citi_core.Dto;

namespace citi_core.Interfaces
{
    public interface ICsvService
    {
        string GenerateTransactionReportCsv(TransactionReportResponse report);
    }
}
