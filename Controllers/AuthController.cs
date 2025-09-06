using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoardAPI.Data;
using NexusBoardAPI.Models;
using NexusBoardAPI.Models.DTO;
using NexusBoardAPI.Services;
using System.Security.Cryptography;
using System.Transactions;

namespace NexusBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public AuthController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("register/user")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> RegisterUser(UserDto dto)
        {
            var adminUsername = User.Identity?.Name;
            var admin = await _context.Users.SingleOrDefaultAsync(u => u.Username == adminUsername && u.Role == UserRole.Admin);
            if (admin == null)
                return Problem(
                    detail: "Only admins can register a user.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized"
                );

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return Problem(
                    detail: "Username already exists.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "BadRequest"
                );

            if(dto.Role == UserRole.Admin || dto.Role == UserRole.MasterAdmin)
                return Problem(
                    detail: "Cannot assign Admin or MasterAdmin role.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "BadRequest"
                );

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _authService.HashPassword(dto.Password),
                AdminId = admin.Id,
                Role = dto.Role ?? UserRole.Developer
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new UserProfile
            {
                FullName = dto.Username,
                UserId = user.Id,
                User = user
            };
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(UserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return Problem(
                     detail: "Username already exists.",
                     statusCode: StatusCodes.Status400BadRequest,
                     title: "BadRequest"
                );

            var user = new User             
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _authService.HashPassword(dto.Password),
                Role = UserRole.Admin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if(user == null || user.PasswordHash != _authService.HashPassword(dto.Password))
                return Problem(
                   detail: "Invalid username or password.",
                   statusCode: StatusCodes.Status401Unauthorized,
                   title: "Unauthorized"
                );

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => 
                u.Username == dto.Username &&
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return Problem(
                     detail: "Invalid token or token has expired.",
                     statusCode: StatusCodes.Status400BadRequest,
                     title: "BadRequest"
                );

            user.PasswordHash = _authService.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password successfully reset."});
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username && u.Email == dto.Email);
            if(user == null)
                return Problem(
                     detail: "User with the provided username and email does not exist.",
                     statusCode: StatusCodes.Status400BadRequest,
                     title: "BadRequest"
                );

            user.PasswordResetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();
            // In a real application, send the token via email here.
            return Ok(new { message = "Password reset token generated. Please check your email for further instructions." });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var username = User.Identity?.Name;
            if(string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if(user == null || user.PasswordHash != _authService.HashPassword(dto.CurrentPassword))
                return Problem(
                     detail: "Current password is incorrect.",
                     statusCode: StatusCodes.Status400BadRequest,
                     title: "BadRequest"
                );

            user.PasswordHash = _authService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
