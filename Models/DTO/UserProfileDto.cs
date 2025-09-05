namespace NexusBoardAPI.Models.DTO
{
    public class UserProfileDto
    {
        public required string FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
