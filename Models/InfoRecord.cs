using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DowntimeTracker.Models
{
    public class InfoRecord
    {
        [Required]
        public int InfoRecordId { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Kategoria")]
        [Column(TypeName = "VARCHAR(256)")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Powód")]
        [Column(TypeName = "VARCHAR(256)")]
        public string CorporateReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Kategoria - 4M")]
        [Column(TypeName = "VARCHAR(32)")]
        public string M4Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Podlega Roszczeniu")]
        public bool IsClaimable { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Typ")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Przykład")]
        [Column(TypeName = "VARCHAR(1024)")]
        public string Example { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Tłumaczenie")]
        [Column(TypeName = "VARCHAR(1024)")]
        public string Explanation { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Widoczna")]
        public bool IsVisible { get; set; } = true;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Osoba Odpowiedzialna")]
        [Column(TypeName = "VARCHAR(32)")]
        public string Responsible { get; set; } = string.Empty;
    }
}
