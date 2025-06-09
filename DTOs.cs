namespace QuickChat.Server.Models
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ChatDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsGroup { get; set; }
        public IEnumerable<UserDto> Participants { get; set; }
        public MessageDto LastMessage { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public UserDto Sender { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
