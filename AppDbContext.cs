using Microsoft.EntityFrameworkCore;
using QuickChat.Server.Models;

namespace QuickChat.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<UserChat> UserChats { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserChat>()
                .HasKey(uc => new { uc.UserId, uc.ChatId });

            modelBuilder.Entity<UserChat>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserChats)
                .HasForeignKey(uc => uc.UserId);

            modelBuilder.Entity<UserChat>()
                .HasOne(uc => uc.Chat)
                .WithMany(c => c.UserChats)
                .HasForeignKey(uc => uc.ChatId);

            // Индексы для оптимизации
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.ChatId);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.SenderId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
