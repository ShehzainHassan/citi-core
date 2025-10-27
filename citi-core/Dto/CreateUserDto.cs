namespace citi_core.Dto
{
    public class CreateUserDto
    {
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }

}
