namespace QuickChat.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime LastOnline { get; set; }
        public bool IsOnline { get; set; }

        public ICollection<UserChat> UserChats { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
