namespace citi_core.Dto
{
    public class AddCardRequestDto
    {
        public Guid CardId { get; set; }
        public string Status { get; set; } = default!;
    }
}
