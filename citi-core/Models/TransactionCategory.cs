using citi_core.Enums;
using citi_core.Enums.citi_core.Enums;
using System.ComponentModel.DataAnnotations;

namespace citi_core.Models
{
    public class TransactionCategory : BaseEntity
    {
        [Key]
        public Guid TransactionCategoryId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public TransactionCategoryType Type { get; set; }
        [Required]
        public bool IsSystem { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}