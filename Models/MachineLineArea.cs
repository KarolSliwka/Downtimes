using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DowntimeTracker.Models
{
    public class MachineLineArea
    {
        [Key]
        public int MachineLineAreaId { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Typ")]
        public int? Type { get; set; } = 0;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Główna Organizacja")]
        [Column(TypeName = "VARCHAR(256)")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Nazwa")]
        [Column(TypeName = "VARCHAR(128)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [Display(Name = "Tryb Pracy")]
        public int OperatingTime { get; set; }

        public List<Downtime>? Downtime { get; set; }
    }
}