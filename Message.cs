namespace QuickChat.Server.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
