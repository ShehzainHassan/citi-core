using citi_core.Enums;
using System.ComponentModel.DataAnnotations;

namespace citi_core.Dto
{
    public class TransactionReportRequest
    {
        public Guid? AccountId { get; set; }
        public Guid? CardId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TransactionType>? TransactionTypes { get; set; }
        public List<Guid>? Categories { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public TransactionStatus? Status { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}