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
  
    }
}
