using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickChat.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username is already taken");
            }

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Name = registerDto.Name,
                AvatarUrl = "default-avatar.png",
                LastOnline = DateTime.UtcNow,
                IsOnline = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    AvatarUrl = user.AvatarUrl,
                    IsOnline = user.IsOnline
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password");
            }

            user.LastOnline = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    AvatarUrl = user.AvatarUrl,
                    IsOnline = user.IsOnline
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
