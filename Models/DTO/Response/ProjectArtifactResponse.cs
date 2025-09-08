namespace NexusBoardAPI.Models.DTO.Response
{
    public class ProjectArtifactResponse
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public required string Version { get; set; }
        public string? ReleaseNotes { get; set; }
        public DateTime UploadAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
    }
}
