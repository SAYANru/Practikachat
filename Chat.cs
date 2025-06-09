namespace QuickChat.Server.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsGroup { get; set; }

        public ICollection<UserChat> UserChats { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

}
