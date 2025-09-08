namespace NexusBoardAPI.Models.DTO.Response
{
    public class ProjectThreadMessageResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    }
}
