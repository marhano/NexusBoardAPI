namespace NexusBoardAPI.Models.DTO.Parameter
{
    public class ProjectArtifactParam
    {
        public IFormFile File { get; set; } = null!;
        public string Version { get; set; } = string.Empty;
        public string? ReleaseNotes { get; set; }
    }
}
