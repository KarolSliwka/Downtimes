using System.ComponentModel.DataAnnotations;

namespace DowntimeTracker.Models
{
    public class UserLog
    {
        [Key]
        public int UserLogId { get; set; }

        [Required]
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public DateTime LastLogin { get; set; }

        [Required]
        public DateTime CurrentLogin { get; set; }

        public int DaysSinceLastLogin()
        {
            return (CurrentLogin - LastLogin).Days;
        }
    }
}