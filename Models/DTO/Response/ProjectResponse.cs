namespace NexusBoardAPI.Models.DTO.Response
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? ApplicationId { get; set; }
        public string? AccessToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Version { get; set; }

        public int CreateById { get; set; }
        public string CreateBy { get; set; } = string.Empty;

        public List<string> Admins { get; set; } = new();
        public List<string> Users { get; set; } = new();

        public ICollection<ProjectArtifactResponse> Artifacts { get; set; } = new List<ProjectArtifactResponse>();
        public ICollection<ProjectThreadResponse> Threads { get; set; } = new List<ProjectThreadResponse>();
    }
}
