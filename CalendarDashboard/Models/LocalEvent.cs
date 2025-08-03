using System.ComponentModel.DataAnnotations;
using Google.Apis.Calendar.v3.Data;

namespace CalendarDashboard.Models
{
    public class LocalEvent
    {
        //REQUIRED PROPERTIES
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string EventId { get; set; } = null!;

        [Required]
        public string CalendarId { get; set; } = null!;
        
        [Required]
        public EventDateTime StartTime { get; set; } = null!;
        
        [Required]
        public EventDateTime EndTime { get; set; } = null!;


        //COMMON OPTIONAL PROPERTIES

        public string? Name { get; set; }

        public string? Description { get; set; }
       
        public string? Location { get; set; }

        public string? Status { get; set; }
        
        public List<EventAttendee>? Attendees { get; set; }
       

        //LESS COMMONLY USED PROPERTIES
        public string? Notes { get; set; }

    }
}
