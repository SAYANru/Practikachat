using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.Security.Claims;

namespace QuickChat.Server.Controllers
{
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetChats()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var chats = await _context.UserChats
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.Chat)
                .Select(c => new ChatDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsGroup = c.IsGroup,
                    Participants = c.UserChats
                        .Select(uc => new UserDto
                        {
                            Id = uc.User.Id,
                            Username = uc.User.Username,
                            Name = uc.User.Name,
                            AvatarUrl = uc.User.AvatarUrl,
                            IsOnline = uc.User.IsOnline
                        }),
                    LastMessage = c.Messages
                        .OrderByDescending(m => m.SentAt)
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
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.LastMessage != null ? c.LastMessage.SentAt : DateTime.MinValue)
                .ToListAsync();

            return Ok(chats);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] int[] participantIds)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var allParticipants = participantIds.Append(userId).Distinct().ToList();

            if (allParticipants.Count < 2)
            {
                return BadRequest("Chat must have at least 2 participants");
            }

            // Проверяем, существует ли уже такой чат
            var existingChat = await _context.Chats
                .Where(c => !c.IsGroup &&
                       c.UserChats.Count == allParticipants.Count &&
                       c.UserChats.All(uc => allParticipants.Contains(uc.UserId)))
                .FirstOrDefaultAsync();

            if (existingChat != null)
            {
                return Ok(new ChatDto
                {
                    Id = existingChat.Id,
                    Name = existingChat.Name,
                    IsGroup = existingChat.IsGroup,
                    Participants = existingChat.UserChats
                        .Select(uc => new UserDto
                        {
                            Id = uc.User.Id,
                            Username = uc.User.Username,
                            Name = uc.User.Name,
                            AvatarUrl = uc.User.AvatarUrl,
                            IsOnline = uc.User.IsOnline
                        })
                });
            }

            // Создаем новый чат
            var chat = new Chat
            {
                IsGroup = allParticipants.Count > 2,
                Name = allParticipants.Count > 2 ? "Group Chat" : null,
                UserChats = allParticipants.Select(id => new UserChat { UserId = id }).ToList()
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return Ok(new ChatDto
            {
                Id = chat.Id,
                Name = chat.Name,
                IsGroup = chat.IsGroup,
                Participants = chat.UserChats
                    .Select(uc => new UserDto
                    {
                        Id = uc.User.Id,
                        Username = uc.User.Username,
                        Name = uc.User.Name,
                        AvatarUrl = uc.User.AvatarUrl,
                        IsOnline = uc.User.IsOnline
                    })
            });
        }
    }
}
