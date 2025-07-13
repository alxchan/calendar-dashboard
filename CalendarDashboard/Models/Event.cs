using System.ComponentModel.DataAnnotations;

namespace CalendarDashboard.Models
{
    public class Event
    {
        public int Id {  get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Place { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }
    }
}
