using citi_core.Enums;
using citi_core.Models;
using System.ComponentModel.DataAnnotations;

public class CardAuditLog : BaseEntity
{
    [Key]
    public Guid CardAuditLogId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CardId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public CardStatus PreviousStatus { get; set; }

    [Required]
    public CardStatus NewStatus { get; set; }

    [MaxLength(512)]
    public string Reason { get; set; } = default!;

    public Card Card { get; set; } = null!;
    public User User { get; set; } = null!;
}