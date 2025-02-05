using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DowntimeTracker.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "User ID")]
        [Column(TypeName = "VARCHAR(128)")]
        public string UserAd { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Imię & Naziwsko")]
        [Column(TypeName = "VARCHAR(512)")]
        public string NameSurname { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Poziom Dostępu")]
        [Column(TypeName = "VARCHAR(128)")]
        public string AccessLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Dostęp do HRM?")]
        public bool HrmAvailable { get; set; }

        public List<Hrm>? Hrm { get; set; }
        public List<Downtime>? Downtime { get; set; }
        public List<UserLog>? UserLogs { get; set; }
    }
}
