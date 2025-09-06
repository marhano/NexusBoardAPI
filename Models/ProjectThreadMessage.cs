namespace NexusBoardAPI.Models
{
    public class ProjectThreadMessage
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public ProjectThread Thread { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    }
}
