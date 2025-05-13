using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;
using Task = SharedLibrary.Models.Task;

namespace SharedLibrary
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all tables
        public DbSet<Users> DBUsers { get; set; }
        public DbSet<Contact> DBContacts { get; set; }
        public DbSet<Deal> DBDeal { get; set; }
        public DbSet<Task> DBTask { get; set; }
        public DbSet<ChatChannel> DBChatChannel { get; set; }
        public DbSet<ChatMessage> DBChatMessage { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names (if different from class names)
            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Contact>().ToTable("Contact");
            modelBuilder.Entity<Deal>().ToTable("Deal");
            modelBuilder.Entity<Task>().ToTable("Task");
            modelBuilder.Entity<ChatChannel>().ToTable("ChatChannel");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessage");

            // Relationships
            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Contact)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ContactID);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Contact)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ContactID);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.Deal)
                .WithMany(d => d.Tasks)
                .HasForeignKey(t => t.DealID);

            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.User)
                .WithMany(u => u.ChatChannels)
                .HasForeignKey(cc => cc.UserID);

            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.Contact)
                .WithMany(c => c.ChatChannels)
                .HasForeignKey(cc => cc.ContactID);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Channel)
                .WithMany(c => c.ChatMessages)
                .HasForeignKey(cm => cm.ChannelID);

            // Foreign key mappings
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Deal>()
                .HasOne(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔁 AutoInclude navigation properties (EF Core 5+)
            modelBuilder.Entity<Contact>().Navigation(c => c.CreatedByUser).AutoInclude();
            modelBuilder.Entity<Contact>().Navigation(c => c.UpdatedByUser).AutoInclude();

            modelBuilder.Entity<Deal>().Navigation(d => d.CreatedByUser).AutoInclude();
            modelBuilder.Entity<Deal>().Navigation(d => d.UpdatedByUser).AutoInclude();

            modelBuilder.Entity<Task>().Navigation(t => t.CreatedByUser).AutoInclude();
            modelBuilder.Entity<Task>().Navigation(t => t.UpdatedByUser).AutoInclude();

            modelBuilder.Entity<ChatMessage>().Navigation(m => m.CreatedByUser).AutoInclude();

        }
    }
}
