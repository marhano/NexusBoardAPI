namespace NexusBoardAPI.Models.DTO.Response
{
    public class UserProfileResponse
    {
        public required string FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
