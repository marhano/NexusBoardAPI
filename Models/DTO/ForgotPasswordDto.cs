namespace NexusBoardAPI.Models.DTO
{
    public class ForgotPasswordDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }

    }
}
