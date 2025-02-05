using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DowntimeTracker.Models
{
    public class Hrm
    {
        [Key]
        [Display(Name = "HRM No.")]
        public int HrmId { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Week { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Główna Organizacja")]
        [Column(TypeName = "VARCHAR(256)")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Klient")]
        [Column(TypeName = "VARCHAR(128)")]
        public string? Customer { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Kategoria")]
        [Column(TypeName = "VARCHAR(128)")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Powód")]
        [Column(TypeName = "VARCHAR(128)")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Data Rozpoczęcia")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Data Zakończenia")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? EndTime { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Ilość Pracowników")]
        public int EmployeeQty { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Godziny (razem)")]
        public Decimal TotalHours { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Komentarz")]
        [Column(TypeName = "VARCHAR(500)")]
        public string Commentary { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Pracownik UA")]
        public bool IsUA { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public int Status { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Utworzone Przez")]
        [Column(TypeName = "VARCHAR(256)")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Data Utworzenia")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Zamknięte Przez")]
        public int? ClosedById { get; set; }

        [Display(Name = "Data Zamknięcia")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? ClosedAt { get; set; }

        [Display(Name = "Osoba Akceptująca")]
        [Column(TypeName = "VARCHAR(256)")]
        public string? Approver { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Status Akceptacji")]
        public int ApprovalStatus { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Użytkownik")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Display(Name = "Id Przestoju")]
        public int? DowntimeId { get; set; }
        public Downtime? Downtime { get; set; }
    }
}