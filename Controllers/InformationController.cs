using DowntimeTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DowntimeTracker.Controllers
{
    [Authorize(Policy = "AllUsers")]
    public class InformationController : Controller
    {
        private readonly ILogger<InformationController> _logger;
        private readonly TCZNT5000 _context;

        public InformationController(ILogger<InformationController> logger, TCZNT5000 context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return _context.InfoRecords != null ?
                 View(await _context.InfoRecords
                    .Where(o => o.IsVisible == true)
                    .OrderBy(o => o.Category)
                    .ThenBy(o => o.CorporateReason)
                    .ThenBy(o => o.Type)
                    .ThenBy(o => o.IsClaimable)
                    .ThenByDescending(o => o.Responsible)
                    .Distinct()
                    .ToListAsync()) :
                Problem("Entity set 'TCZNT5000.InfoRecord'  is null.");

        }
    }
}