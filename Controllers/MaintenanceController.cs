using Microsoft.AspNetCore.Mvc;

namespace DiscrepancyReport.Controllers
{
    public class MaintenanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}