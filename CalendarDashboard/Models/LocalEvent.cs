using System.ComponentModel.DataAnnotations;

namespace CalendarDashboard.Models
{
    public class LocalEvent
    {
        [Required]
        public int Id {  get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public bool Confirmed { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }
    }
}
