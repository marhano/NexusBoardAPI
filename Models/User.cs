namespace NexusBoardAPI.Models
{
    public enum UserRole
    {
        MasterAdmin,
        Admin,
        Developer,
        SeniorDeveloper,
        QA,
        LeadQA
    }
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public UserRole Role { get; set; } 
        public UserProfile? Profile { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public int? AdminId { get; set; }
        public User? Admin { get; set; }
    }
}
