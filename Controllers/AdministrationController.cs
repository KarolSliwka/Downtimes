using DowntimeTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DowntimeTracker.Controllers
{
    [Authorize]
    public class AdministrationController : Controller
    {
        private readonly TCZNT5000 _context;

        public AdministrationController(TCZNT5000 context)
        {
            _context = context;
        }

        // GET: ELRecord
        public IActionResult Index()
        {
            return View();
        }
    }
}
