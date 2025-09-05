using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoardAPI.Data;
using NexusBoardAPI.Models;
using NexusBoardAPI.Models.DTO;

namespace NexusBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("{username}/profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string username, UserProfileDto dto)
        {
            var currentUser = User.Identity?.Name;
            var isAdmin = User.IsInRole(UserRole.Admin.ToString()) || User.IsInRole(UserRole.MasterAdmin.ToString());
            if (!isAdmin && !string.Equals(currentUser, username, StringComparison.OrdinalIgnoreCase))
                return Forbid("You can only update your own profile.");

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("User not found.");

            if (user.Profile == null)
                return NotFound("User profile not found. Create a new profile first.");

            user.Profile.FullName = dto.FullName;
            user.Profile.AvatarUrl = dto.AvatarUrl;
            user.Profile.Bio = dto.Bio;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{username}/profile")]
        [Authorize]
        public async Task<IActionResult> CreateProfile(string username, UserProfileDto dto)
        {
            var currentUser = User.Identity?.Name;
            var isAdmin = User.IsInRole(UserRole.Admin.ToString()) || User.IsInRole(UserRole.MasterAdmin.ToString());
            if (!isAdmin && !string.Equals(currentUser, username, StringComparison.OrdinalIgnoreCase))
                return Forbid("You can only update your own profile.");

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("User not found.");

            if (user.Profile != null)
                return BadRequest("User profile already exists. Use PUT to update.");

            var profile = new UserProfile
            {
                FullName = dto.FullName,
                AvatarUrl = dto.AvatarUrl,
                Bio = dto.Bio,
                UserId = user.Id,
                User = user
            };

            _context.Add(profile);
            await _context.SaveChangesAsync();

            return Ok("User profile upserted successfully.");
        }

        [HttpGet("/api/profile")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == currentUser);

            if (user == null)
                return NotFound("User not found.");

            if (user.Profile == null)
                return NotFound("User profile not found.");

            return Ok(new
            {
                user.Username,
                user.Email,
                user.Role,
                Profile = user.Profile == null ? null : new UserProfileDto
                {
                    FullName = user.Profile.FullName,
                    AvatarUrl = user.Profile.AvatarUrl,
                    Bio = user.Profile.Bio
                }
            });
        }

        [HttpGet("{username}/profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile(string username)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound("User not found.");

            if (user.Profile == null)
                return NotFound("User profile not found.");

            var dto = new UserProfileDto
            {
                FullName = user.Profile.FullName,
                AvatarUrl = user.Profile.AvatarUrl,
                Bio = user.Profile.Bio
            };
            return Ok(dto);
        }

        [HttpGet("/api/profiles")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetAllProfile()
        {
            var currentAdminUsername = User.Identity?.Name;
            if(string.IsNullOrEmpty(currentAdminUsername))
                return Unauthorized("User is not authenticated.");

            var currentAdmin = await _context.Users.SingleOrDefaultAsync(u => u.Username == currentAdminUsername && u.Role == UserRole.Admin);

            if (currentAdmin == null)
                return Forbid("Only admins can access this resource.");

            var profiles = await _context.UserProfiles
                .Include(p => p.User)
                .Where(p => p.User.AdminId == currentAdmin.Id)
                .Select(p => new
                {
                    p.User.Username,
                    p.User.Email,
                    p.User.Role,
                    Profile = new UserProfileDto
                    {
                        FullName = p.FullName,
                        AvatarUrl = p.AvatarUrl,
                        Bio = p.Bio
                    }
                })
                .ToListAsync();

            return Ok(profiles);
        }

        [HttpDelete("{username}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var currentAdminUsername = User.Identity?.Name;
            if(string.IsNullOrEmpty(currentAdminUsername))
                return Unauthorized(new { error = "Unauthorized", message = "User is not authenticated."});

            var currentAdmin = await _context.Users.SingleOrDefaultAsync(u => u.Username == currentAdminUsername && u.Role == UserRole.Admin);
            if(currentAdmin == null)
                return Unauthorized(new { error = "Forbidden", message = "Only admins can delete users."});

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if(user == null)
                return NotFound(new { error = "User not found.", message = $"No user with username '{username}' exists." });
            
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully." });
        }
    }
}
