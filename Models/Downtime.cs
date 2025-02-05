using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DowntimeTracker.Models
{
    public class Downtime
    {
        [Key]
        public int DowntimeId { get; set; }

        [Display(Name = "Rok")]
        public int Year { get; set; }

        [Display(Name = "Tydzień")]
        public int Week { get; set; }

        [Column(TypeName = "VARCHAR(128)")]
        public string Site { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Data Rozpoczęcia")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime EventStartTime { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Data Zakończenia")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime EventEndTime { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Główna Organizacja")]
        [Column(TypeName = "VARCHAR(256)")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Klient")]
        [Column(TypeName = "VARCHAR(128)")]
        public string Customer { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Kategoria")]
        [Column(TypeName = "VARCHAR(128)")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Powód")]
        [Column(TypeName = "VARCHAR(128)")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Ilość Pracowników")]
        public int PeopleAffected { get; set; }

        [Display(Name = "Godziny (razem)")]
        public Decimal TotalHours { get; set; }

        [Display(Name = "Podlega Roszczeniu")]
        public bool IsClaimable { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Komentarz")]
        public string Commentary { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Obszar/Maszyna/Linia")]
        public int? MachineLineAreaId { get; set; }
        public MachineLineArea? MachineLineArea { get; set; }

        [Column(TypeName = "VARCHAR(128)")]
        public string? ApproverEmailADID { get; set; }

        [Display(Name = "Utworzone Przez")]
        [Column(TypeName = "VARCHAR(256)")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Użytkownik")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Display(Name = "Data Utworzenia")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Utworzone Roszczenie")]
        public bool Claimed { get; set; }

        [Column(TypeName = "VARCHAR(256)")]
        [Display(Name = "Roszczenie Utworzone Przez")]
        public string? ClaimedBy { get; set; }

        [Display(Name = "Data Roszczenia")]
        public DateTime? ClaimedAt { get; set; }

        public List<Hrm>? Hrm { get; set; }
    }
}
