using FhirProject.Api.Data;
using FhirProject.Api.DTOs;
using FhirProject.Api.Models.entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FhirProject.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("Username is required");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                // Return existing user, ignore role from request
                return Ok(new LoginResponseDto
                {
                    Success = true,
                    UserId = existingUser.Id,
                    Role = existingUser.Role.ToString()
                });
            }

            // Create new user with selected role
            var newUser = new UserEntity
            {
                Username = request.Username,
                Role = request.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponseDto
            {
                Success = true,
                UserId = newUser.Id,
                Role = newUser.Role.ToString()
            });
        }
    }
}