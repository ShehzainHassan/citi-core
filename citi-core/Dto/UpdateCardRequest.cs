namespace citi_core.Dto
{
    public class UpdateCardRequest
    {
        public string CardName { get; set; } = default!;
        public Guid AccountId { get; set; }
    }
}
