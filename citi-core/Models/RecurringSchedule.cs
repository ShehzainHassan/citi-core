using citi_core.Enums;
using System.ComponentModel.DataAnnotations;
namespace citi_core.Models
{
    public class RecurringSchedule : BaseEntity
    {
        [Key]
        public Guid RecurringScheduleId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public TransactionType TransactionType { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public RecurringFrequency Frequency { get; set; }
        public DateTime NextExecutionDate { get; set; }
        public DateTime? LastExecutionDate { get; set; }
        public int? DayOfMonth { get; set; }

        [Required]
        [MaxLength(20)]
        public RecurringStatus Status { get; set; } = RecurringStatus.Active;

        [MaxLength(200)]
        public string? Description { get; set; }
        public User User { get; set; } = null!;
    }


}



