namespace NexusBoardAPI.Models
{
    public class ProjectThread
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ProjectThreadMessage> Messages { get; set; } = new List<ProjectThreadMessage>();
    }
}
