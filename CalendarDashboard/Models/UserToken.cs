using System.ComponentModel.DataAnnotations;

namespace CalendarDashboard.Models
{
    public class UserToken
    {

        [Key]
        public string UserId { get; set; } = null!;

        public string? AccessToken { get; set; } = null;
        
        [Required]
        public string? RefreshToken { get; set; } = null;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}
