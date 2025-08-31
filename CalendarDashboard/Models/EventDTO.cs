using System.ComponentModel.DataAnnotations;
using Google.Apis.Calendar.v3.Data;

namespace CalendarDashboard.Models
{
    public class EventDTO
    {

        [Required]
        public string Email { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }


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
