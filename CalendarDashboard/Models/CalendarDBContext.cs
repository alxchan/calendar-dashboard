using Microsoft.EntityFrameworkCore;

namespace CalendarDashboard.Models
{

    public class CalendarDBContext : DbContext
    {
        public CalendarDBContext(DbContextOptions<CalendarDBContext> options) : base(options) 
        { 
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Task> Tasks { get; set; }
  
    }
}
