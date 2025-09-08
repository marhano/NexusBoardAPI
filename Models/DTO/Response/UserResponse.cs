namespace NexusBoardAPI.Models.DTO.Response
{
    public class UserResponse
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public UserRole Role { get; set; } 
        public UserProfileResponse? Profile { get; set; }
    }
}
