using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DowntimeTracker.Models
{
    [Keyless]
    public class CategoryReason
    {
        [Display(Name = "Kategoria")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Powód Korporacyjny")]
        public string CorporateReason { get; set; } = string.Empty;

        [Display(Name = "Podlega Roszczeniu")]
        public string IsClaimable { get; set; } = string.Empty;

        [Display(Name = "Typ Powodu")]
        public string ReasonType { get; set; } = string.Empty;
    }
}
