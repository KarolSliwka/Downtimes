using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DowntimeTracker.Models
{
    [Table("Personel")]
    public class Personel
    {
        [Key]
        public int EmpId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR(256)")]
        public string EmpNameSurname { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VARCHAR(64)")]
        public string EmpAd { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VARCHAR(256)")]
        public string EmpPosition { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VARCHAR(256)")]
        public string SupNameSurname { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VARCHAR(64)")]
        public string SupAd { get; set; } = string.Empty;
    }
}
