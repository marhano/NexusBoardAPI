using Microsoft.AspNetCore.Authorization;
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
                return Problem(
                    detail: "You can only update your own profile.",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden"
                );

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Problem(
                    detail: "User not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

            if (user.Profile == null)
                return Problem(
                    detail: "User does not have a profile. Use POST to create a new profile.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "BadRequest"
                );

            user.Profile.FullName = dto.FullName;
            user.Profile.AvatarUrl = dto.AvatarUrl;
            user.Profile.Bio = dto.Bio;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpPost("{username}/profile")]
        [Authorize]
        public async Task<IActionResult> CreateProfile(string username, UserProfileDto dto)
        {
            var currentUser = User.Identity?.Name;
            var isAdmin = User.IsInRole(UserRole.Admin.ToString()) || User.IsInRole(UserRole.MasterAdmin.ToString());
            if (!isAdmin && !string.Equals(currentUser, username, StringComparison.OrdinalIgnoreCase))
                return Problem(
                    detail: "You can only create your own profile.",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden"
                );

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Problem(
                    detail: "User not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

            if (user.Profile != null)
                return Problem(
                    detail: "User already has a profile. Use PUT to update the existing profile.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "BadRequest"
                );

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

            return Ok(new { message = "Profile created successfully." });
        }

        [HttpGet("/api/profile")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
                return Problem(
                    detail: "User is not authenticated.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Username == currentUser);
            if (user == null)
                return Problem(
                    detail: "Authenticated user not found in the database.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

            if (user.Profile == null)
                return Problem(
                    detail: "User does not have a profile.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

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
                return Problem(
                    detail: $"No user with username '{username}' exists.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

            if (user.Profile == null)
                return Problem(
                    detail: $"User '{username}' does not have a profile.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

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
                return Problem(
                    detail: "User is not authenticated.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );

            var currentAdmin = await _context.Users.SingleOrDefaultAsync(u => u.Username == currentAdminUsername && u.Role == UserRole.Admin);
            if (currentAdmin == null)
                return Problem(
                   detail: "Only admins can view all user profiles.",
                   statusCode: StatusCodes.Status401Unauthorized,
                   title: "Unauthorized"
                );

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
                return Problem(
                    detail: "User is not authenticated.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );

            var currentAdmin = await _context.Users.SingleOrDefaultAsync(u => u.Username == currentAdminUsername && u.Role == UserRole.Admin);
            if(currentAdmin == null)
                return Problem(
                   detail: "Only admins can delete users.",
                   statusCode: StatusCodes.Status401Unauthorized,
                   title: "Unauthorized"
                );

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if(user == null)
                return Problem(
                    detail: $"No user with username '{username}' exists.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

            if(user.AdminId != currentAdmin.Id)
                return Problem(
                    detail: "You can only delete users under your administration.",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden"
                );

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }

        [HttpPut("{username}/role")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> ChangeUserRole(string username, ChangeUserRoleDto dto)
        {
            var currentAdminUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentAdminUsername))
                return Problem(
                    detail: "User is not authenticated.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );

            var currentAdmin = await _context.Users.SingleOrDefaultAsync(u => u.Username == currentAdminUsername && u.Role == UserRole.Admin);
            if(currentAdmin == null)
                return Problem(
                   detail: "Only admins can change user roles.",
                   statusCode: StatusCodes.Status401Unauthorized,
                   title: "Unauthorized"
               );

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Problem(
                    detail: $"No user with username '{username}' exists.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "NotFound"
                );

            if(user.AdminId != currentAdmin.Id)
                return Problem(
                    detail: "You can only change roles for users under your administration.",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden"
                );

            if (!Enum.TryParse<UserRole>(dto.Role, out var newRole))
                return Problem(
                    detail: $"Role '{dto.Role}' is not valid. Valid roles are: Developer, SeniorDeveloper, QA, LeadQA.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "BadRequest"
                );

            var validTransitions = new Dictionary<UserRole, List<UserRole>>
            {
                { UserRole.Developer, new List<UserRole> { UserRole.SeniorDeveloper } },
                { UserRole.SeniorDeveloper, new List<UserRole> { UserRole.Developer } },
                { UserRole.QA, new List<UserRole> { UserRole.LeadQA } },
                { UserRole.LeadQA, new List<UserRole> { UserRole.QA } }
            };

            if (!validTransitions.TryGetValue(user.Role, out var allowedRoles) || !allowedRoles.Contains(newRole))
                return Problem(
                    detail: $"Invalid role transition from '{user.Role}' to '{newRole}'.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "BadRequest"
                );


            user.Role = newRole;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User role updated successfully." });
        }
    }
}
