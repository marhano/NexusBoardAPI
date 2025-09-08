using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoardAPI.Data;
using NexusBoardAPI.Models;
using NexusBoardAPI.Models.DTO.Parameter;
using NexusBoardAPI.Models.DTO.Response;

namespace NexusBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtifactsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArtifactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/projects/{projectId}/artifacts")]
        [Authorize]
        public async Task<IActionResult> UploadArtifact(int projectId, [FromForm] ProjectArtifactParam dto)
        {
            if(dto.File == null || dto.File.Length == 0)
                return Problem(
                    detail: "No file uploaded",
                    title: "BadRequest",
                    statusCode: StatusCodes.Status400BadRequest
                );

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if(project == null)
                return Problem(
                    detail: "Project not found",
                    title: "NotFound",
                    statusCode: StatusCodes.Status404NotFound
                );

            var uploadsFolder = Path.Combine("wwwroot", "artifacts", projectId.ToString());
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, dto.File.FileName);

            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var artifact = new ProjectArtifact
            {
                ProjectId = projectId,
                FileName = dto.File.FileName,
                FilePath = filePath,
                Version = dto.Version,
                ReleaseNotes = dto.ReleaseNotes,
                UploadAt = DateTime.UtcNow,
                UploadedBy = User.Identity?.Name ?? "Unknown"
            };

            _context.Add(artifact);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Artifact uploaded successfully." });
        }

        [HttpGet("/api/projects/{projectId}/artifacts")]
        [Authorize]
        public async Task<IActionResult> ListArtifacts(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Artifacts)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if(project == null)
                return Problem(
                    detail: "Project not found",
                    title: "NotFound",
                    statusCode: StatusCodes.Status404NotFound
                );
            var artifacts = project.Artifacts.Select(a => new ProjectArtifactResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                Version = a.Version,
                ReleaseNotes = a.ReleaseNotes,
                UploadAt = a.UploadAt,
                UploadedBy = a.UploadedBy
            }).ToList();
            return Ok(artifacts);
        }

        [HttpGet("/api/projects/{projectId}/artifacts/{artifactId}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadArtifact(int projectId, int artifactId)
        {
            var artifact = await _context.ProjectArtifacts
                .FirstOrDefaultAsync(a => a.Id == artifactId && a.ProjectId == projectId);
            if(artifact == null)
                return Problem(
                    detail: "Artifact not found",
                    title: "NotFound",
                    statusCode: StatusCodes.Status404NotFound
                );

            if (!System.IO.File.Exists(artifact.FilePath))
                return Problem(
                    detail: "File not found on server",
                    title: "NotFound",
                    statusCode: StatusCodes.Status404NotFound
                );
            
            var memory = new MemoryStream();
            using(var stream = new FileStream(artifact.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = "application/octet-stream";
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if(!provider.TryGetContentType(artifact.FileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(memory, contentType, artifact.FileName);
        }
    }
}
