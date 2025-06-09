using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.Security.Claims;

namespace QuickChat.Server.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string search = null)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var query = _context.Users
                .Where(u => u.Id != currentUserId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Username.Contains(search));
            }

            var users = await query
                .OrderBy(u => u.Name)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                    AvatarUrl = u.AvatarUrl,
                    IsOnline = u.IsOnline
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
