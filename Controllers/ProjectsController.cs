using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoardAPI.Data;
using NexusBoardAPI.Models;
using NexusBoardAPI.Models.DTO;
using NexusBoardAPI.Models.DTO.Response;

namespace NexusBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/project")]
        [Authorize]
        public async Task<IActionResult> CreateProject(ProjectDto dto)
        {
            var currentUser = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == currentUser);
            if (user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                ApplicationId = dto.ApplicationId,
                Version = dto.Version,
                AccessToken = dto.AccessToken,

                CreateById = user.Id,
                CreateBy = user.Username,

                Admins = new List<string> { user.Username }
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project created successfully" });
        }

        [HttpGet()]
        [Authorize]
        public async Task<IActionResult> GetAllProjects()
        {
            var currentUser = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUser);
            if(user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var projects = await _context.Projects
                .Where(p => p.Admins.Contains(user.Username) || p.Users.Contains(user.Username))
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProject(int id)
        {
            var currentUser = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUser);
            if(user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = await _context.Projects
                .Include(p => p.Artifacts)
                .Include(p => p.Threads)
                .ThenInclude(t => t.Messages)
                .FirstOrDefaultAsync(p => p.Id == id && (p.Admins.Contains(user.Username) || p.Users.Contains(user.Username)));

            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            return Ok(new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ApplicationId = project.ApplicationId,
                AccessToken = project.AccessToken,
                CreatedAt = project.CreatedAt,
                Version = project.Version,
                CreateById = project.CreateById,
                CreateBy = project.CreateBy,
                Admins = project.Admins,
                Users = project.Users,
                Artifacts = project.Artifacts.Select(a => new ProjectArtifactResponse
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    Version = a.Version,
                    ReleaseNotes = a.ReleaseNotes,
                    UploadAt = a.UploadAt,
                    UploadedBy = a.UploadedBy
                }).ToList(),
                Threads = project.Threads.Select(t => new ProjectThreadResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    CreatedBy = t.CreatedBy,
                    CreatedAt = t.CreatedAt,
                    Messages = t.Messages.Select(m => new ProjectThreadMessageResponse
                    {
                        Id = m.Id,
                        Message = m.Message,
                        PostedAt = m.PostedAt,
                        PostedBy = m.PostedBy
                    }).ToList()
                }).ToList()
            });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProject(int id, ProjectDto dto)
        {
            var currentUser = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUser);
            if(user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && (p.Admins.Contains(user.Username) || p.Users.Contains(user.Username)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            project.Name = dto.Name;
            project.Description = dto.Description;
            project.ApplicationId = dto.ApplicationId;
            project.Version = dto.Version;  
            project.AccessToken = dto.AccessToken;

            return Ok(new { message = "Project updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var currentUser = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUser);
            if(user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.Admins.Contains(user.Username));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have admin access",
                    statusCode: StatusCodes.Status404NotFound
                );

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project deleted successfully" });
        }

        [HttpPost("{id}/users")]
        [Authorize]
        public async Task<IActionResult> AddProjectUser(int id, [FromBody] List<string> usernames)
        {
            if(usernames == null || usernames.Count == 0)
                return Problem(
                    title: "BadRequest",
                    detail: "No users specified",
                    statusCode: StatusCodes.Status400BadRequest
                );

            var currentUser = User.Identity?.Name;

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && (p.Admins.Contains(currentUser) || p.Users.Contains(currentUser)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            var usersToAdd = await _context.Users
                .Where(u => usernames.Contains(u.Username) && (!project.Users.Contains(u.Username) || !project.Admins.Contains(u.Username)))
                .Select(u => u.Username)
                .ToListAsync();
            if(usersToAdd.Count == 0)
                return Problem(
                    title: "BadRequest",
                    detail: "No valid users to add",
                    statusCode: StatusCodes.Status400BadRequest
                );

            project.Users.AddRange(usersToAdd);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Users added successfully" });
        }

        [HttpDelete("{id}/users/{username}")]
        [Authorize]
        public async Task<IActionResult> RemoveProjectUser(int id, string username)
        {
            if(string.IsNullOrEmpty(username))
                return Problem(
                    title: "BadRequest",
                    detail: "No user specified",
                    statusCode: StatusCodes.Status400BadRequest
                );
            var currentUser = User.Identity?.Name;

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && (p.Admins.Contains(currentUser) || p.Users.Contains(currentUser)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            var projectAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUser && project.Admins.Contains(currentUser));
            if(projectAdmin == null)
                return Problem(
                    title: "Forbidden",
                    detail: "Only project admins can remove users",
                    statusCode: StatusCodes.Status403Forbidden
                );

            var userToRemove  = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && (project.Users.Contains(username) || project.Admins.Contains(username)));
            if(userToRemove == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found in project",
                    statusCode: StatusCodes.Status404NotFound
                );

            project.Users.Remove(username);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Users removed successfully" });
        }
        
        [HttpGet("{id}/users")]    
        public async Task<IActionResult> ListProjectUsers(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found",
                    statusCode: StatusCodes.Status404NotFound
                );
            var users = await _context.Users
                .Where(u => project.Users.Contains(u.Username) || project.Admins.Contains(u.Username))
                .Select(u => new UserResponse{ 
                    Id = u.Id, 
                    Username = u.Username, 
                    Email = u.Email, 
                    Role = u.Role,  
                    Profile = u.Profile == null ? null : new UserProfileResponse { 
                        FullName = u.Profile.FullName, 
                        AvatarUrl = u.Profile.AvatarUrl, 
                        Bio = u.Profile.Bio
                    }
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
