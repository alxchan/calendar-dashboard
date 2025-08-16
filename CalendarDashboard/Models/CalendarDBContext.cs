using Microsoft.EntityFrameworkCore;

namespace CalendarDashboard.Models
{

    public class CalendarDBContext : DbContext
    {
        public CalendarDBContext(DbContextOptions<CalendarDBContext> options) : base(options) 
        { 
        }

        public DbSet<LocalEvent> Events { get; set; }
        public DbSet<Task> Tasks { get; set; }

        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalEvent>().HasKey(e => new { e.Id, e.CalendarId, e.EventId});
            modelBuilder.Entity<LocalEvent>().OwnsOne(e => e.StartTime);
            modelBuilder.Entity<LocalEvent>().OwnsOne(e => e.EndTime);
            modelBuilder.Entity<LocalEvent>().OwnsMany(e => e.Attendees, ea =>
            {
                ea.WithOwner().HasForeignKey("UserId", "CalendarId", "EventId");
                ea.Property<int>("Uid");
                ea.HasKey("Uid");
            });
        }
  
    }
}
