using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.Security.Claims;

namespace QuickChat.Server.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] int chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userInChat = await _context.UserChats
                .AnyAsync(uc => uc.UserId == userId && uc.ChatId == chatId);

            if (!userInChat)
            {
                return Forbid();
            }

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    Sender = new UserDto
                    {
                        Id = m.Sender.Id,
                        Username = m.Sender.Username,
                        Name = m.Sender.Name,
                        AvatarUrl = m.Sender.AvatarUrl,
                        IsOnline = m.Sender.IsOnline
                    },
                    Text = m.Text,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages.OrderBy(m => m.SentAt));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageSendDto messageDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userInChat = await _context.UserChats
                .AnyAsync(uc => uc.UserId == userId && uc.ChatId == messageDto.ChatId);

            if (!userInChat)
            {
                return Forbid();
            }

            var message = new Message
            {
                ChatId = messageDto.ChatId,
                SenderId = userId,
                Text = messageDto.Text,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Sender = new UserDto
                {
                    Id = userId,
                    Username = User.FindFirst(ClaimTypes.Name).Value,
                    Name = (await _context.Users.FindAsync(userId)).Name,
                    AvatarUrl = (await _context.Users.FindAsync(userId)).AvatarUrl,
                    IsOnline = true
                },
                Text = message.Text,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            });
        }
    }

    public class MessageSendDto
    {
        public int ChatId { get; set; }
        public string Text { get; set; }
    }
}
