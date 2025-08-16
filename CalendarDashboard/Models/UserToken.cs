using System.ComponentModel.DataAnnotations;

namespace CalendarDashboard.Models
{
    public class UserToken
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Service { get; set; } = null!;
        public string? AccessToken { get; set; } = null;
        
        [Required]
        public string? RefreshToken { get; set; } = null;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}
