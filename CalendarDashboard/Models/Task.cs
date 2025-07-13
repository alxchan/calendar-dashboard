using System.ComponentModel.DataAnnotations;

namespace CalendarDashboard.Models
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public decimal CompletionRate { get; set; }
        
        [Required]
        public Boolean Completed { get; set; }

        public string? Description { get; set; }

        public string? Notes { get; set; }

    }
}
