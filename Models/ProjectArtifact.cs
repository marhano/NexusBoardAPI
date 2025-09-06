namespace NexusBoardAPI.Models
{
    public class ProjectArtifact
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public required string Version { get; set; }
        public string? ReleaseNotes { get; set; }
        public DateTime UploadAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }
}
