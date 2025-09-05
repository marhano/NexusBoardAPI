using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexusBoardAPI.Data;
using NexusBoardAPI.Models;
using NexusBoardAPI.Models.DTO;
using NexusBoardAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
                return Forbid("Only admin can register users.");

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _authService.HashPassword(dto.Password),
                AdminId = admin.Id,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User registered successfully.");
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(UserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username already exists.");

            var user = new User             
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _authService.HashPassword(dto.Password),
                Role = UserRole.Admin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("Admin registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if(user == null || user.PasswordHash != _authService.HashPassword(dto.Password))
                return Unauthorized("Invalid username or password.");

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

            if(user == null)
                return BadRequest("Invalid or expired token.");

            user.PasswordHash = _authService.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == dto.Username && u.Email == dto.Email);
            if(user == null)
                return NotFound("User not found.");

            user.PasswordResetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();
            // In a real application, send the token via email here.
            return Ok();
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
                return BadRequest("Current password is incorrect.");

            user.PasswordHash = _authService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password changed successfully.");
        }
    }
}
