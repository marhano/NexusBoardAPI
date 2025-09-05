namespace NexusBoardAPI.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
