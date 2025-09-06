namespace NexusBoardAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreateById { get; set; }
        public User CreateBy { get; set; } = null!;

        public ICollection<User> Admins { get; set; } = new List<User>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ProjectArtifact> Artifacts { get; set; } = new List<ProjectArtifact>();
        public ICollection<ProjectThread> Threads { get; set; } = new List<ProjectThread>();
    }
}
