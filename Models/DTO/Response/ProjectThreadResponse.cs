namespace NexusBoardAPI.Models.DTO.Response
{
    public class ProjectThreadResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ProjectThreadMessageResponse> Messages { get; set; } = new List<ProjectThreadMessageResponse>();
    }
}
