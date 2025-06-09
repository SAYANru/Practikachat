using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Data;
using QuickChat.Server.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuickChat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = true;
                    user.LastOnline = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await Clients.Others.SendAsync("UserStatusChanged", new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Name = user.Name,
                        AvatarUrl = user.AvatarUrl,
                        IsOnline = true
                    });
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastOnline = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await Clients.Others.SendAsync("UserStatusChanged", new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Name = user.Name,
                        AvatarUrl = user.AvatarUrl,
                        IsOnline = false
                    });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChat(int chatId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChatId == chatId);

            if (userChat != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            }
        }

        public async Task SendMessage(int chatId, string text)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var userChat = await _context.UserChats
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ChatId == chatId);

            if (userChat == null) return;

            var message = new Message
            {
                ChatId = chatId,
                SenderId = userId.Value,
                Text = text,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageDto = new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Sender = new UserDto
                {
                    Id = message.Sender.Id,
                    Username = message.Sender.Username,
                    Name = message.Sender.Name,
                    AvatarUrl = message.Sender.AvatarUrl,
                    IsOnline = message.Sender.IsOnline
                },
                Text = message.Text,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };

            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", messageDto);
        }

        public async Task MarkAsRead(int messageId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return;

            var message = await _context.Messages
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId == userId) return;

            var userInChat = await _context.UserChats
                .AnyAsync(uc => uc.UserId == userId && uc.ChatId == message.ChatId);

            if (!userInChat) return;

            message.IsRead = true;
            await _context.SaveChangesAsync();

            await Clients.Group($"chat_{message.ChatId}").SendAsync("MessageRead", messageId);
        }

        private int? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
