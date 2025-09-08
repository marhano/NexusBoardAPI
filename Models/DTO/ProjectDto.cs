namespace NexusBoardAPI.Models.DTO
{
    public class ProjectDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? ApplicationId { get; set; }
        public string? AccessToken { get; set; }
        public string? Version { get; set; }
    }
}
