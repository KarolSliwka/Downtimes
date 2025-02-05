using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DowntimeTracker.Models
{
    [Keyless]
    public class DepartmentCustomer
    {
        [Display(Name = "Departament")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Klient")]
        public string Customer { get; set; } = string.Empty;
    }
}
