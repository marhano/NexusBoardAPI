using Microsoft.EntityFrameworkCore;
using NexusBoardAPI.Models;

namespace NexusBoardAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectArtifact> ProjectArtifacts { get; set; }
        public DbSet<ProjectThread> ProjectThreads { get; set; }
        public DbSet<ProjectThreadMessage> ThreadMessages { get; set; }
    }
}
