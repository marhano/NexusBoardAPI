using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoardAPI.Data;
using NexusBoardAPI.Models;
using NexusBoardAPI.Models.DTO.Parameter;

namespace NexusBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThreadsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ThreadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/projects/{projectId}/threads")]
        [Authorize]
        public async Task<IActionResult> CreateThread(int projectId, ProjectThreadParam dto)
        {
            var currentUser = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == currentUser);
            if (user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = await _context.Projects
                .Include(p => p.Artifacts)
                .Include(p => p.Threads)
                .ThenInclude(t => t.Messages)
                .FirstOrDefaultAsync(p => p.Id == projectId && (p.Admins.Contains(user.Username) || p.Users.Contains(user.Username)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            var thread = new ProjectThread
            {
                Title = dto.Title,
                Body = dto.Body,
                ProjectId = projectId,
                Project = project,
                CreatedBy = user.Username,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProjectThreads.Add(thread);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Discussion successfully started."});
        }

        [HttpGet("/api/projects/{projectId}/threads")]
        [Authorize]
        public async Task<IActionResult> ListThreads(int projectId)
        {
            var currentUser = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == currentUser);
            if (user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = await _context.Projects
                .Include(p => p.Artifacts)
                .Include(p => p.Threads)
                .ThenInclude(t => t.Messages)
                .FirstOrDefaultAsync(p => p.Id == projectId && (p.Admins.Contains(user.Username) || p.Users.Contains(user.Username)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            var threads = project.Threads
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Body,
                    t.CreatedBy,
                    t.CreatedAt,
                    MessageCount = t.Messages.Count
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            return Ok(threads);
        }

        [HttpGet("/api/projects/{projectId}/threads/{threadId}")]
        [Authorize]
        public async Task<IActionResult> GetThread(int projectId, int threadId)
        {
            var currentUser = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == currentUser);
            if (user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            var project = await _context.Projects
                .Include(p => p.Artifacts)
                .Include(p => p.Threads)
                .ThenInclude(t => t.Messages)
                .FirstOrDefaultAsync(p => p.Id == projectId && (p.Admins.Contains(user.Username) || p.Users.Contains(user.Username)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );

            var thread = project.Threads
                .Where(t => t.Id == threadId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Body,
                    t.CreatedBy,
                    t.CreatedAt,
                    Messages = t.Messages
                        .OrderBy(m => m.PostedAt)
                        .Select(m => new
                        {
                            m.Id,
                            m.Message,
                            m.PostedBy,
                            m.PostedAt
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (thread == null)
                return Problem(
                    title: "NotFound",
                    detail: "Thread not found",
                    statusCode: StatusCodes.Status404NotFound
                );

            return Ok(thread);
        }

        [HttpPost("/api/projects/{projectId}/threads/{threadId}/messages")]
        [Authorize]
        public async Task<IActionResult> PostMessage(int projectId, int threadId, ProjectThreadMessageParam dto)
        {
            var currentUser = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == currentUser);
            if (user == null)
                return Problem(
                    title: "NotFound",
                    detail: "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );
            var project = await _context.Projects
                .Include(p => p.Artifacts)
                .Include(p => p.Threads)
                .ThenInclude(t => t.Messages)
                .FirstOrDefaultAsync(p => p.Id == projectId && (p.Admins.Contains(user.Username) || p.Users.Contains(user.Username)));
            if (project == null)
                return Problem(
                    title: "NotFound",
                    detail: "Project not found or you do not have access",
                    statusCode: StatusCodes.Status404NotFound
                );
            var thread = project.Threads.FirstOrDefault(t => t.Id == threadId);
            if (thread == null)
                return Problem(
                    title: "NotFound",
                    detail: "Thread not found",
                    statusCode: StatusCodes.Status404NotFound
                );
            var message = new ProjectThreadMessage
            {
                ThreadId = threadId,
                Thread = thread,
                Message = dto.Message,
                PostedBy = user.Username,
                PostedAt = DateTime.UtcNow
            };
            _context.ThreadMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message posted successfully." });
        }
    }
}
