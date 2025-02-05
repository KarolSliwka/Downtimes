using AspNetCoreHero.ToastNotification.Abstractions;
using DowntimeTracker.Data;
using DowntimeTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DowntimeTracker.Controllers
{
    [Authorize(Policy = "IsHrm")]
    public class HrmsController : Controller
    {
        private readonly TCZNT5000 _context;
        private readonly TCZNT5000Raptor _raptor;
        private readonly INotyfService _toasts;

        public HrmsController(TCZNT5000 context, TCZNT5000Raptor raptor, INotyfService toast)
        {
            _context = context;
            _raptor = raptor;
            _toasts = toast;
        }

        // GET: Hrms
        public async Task<IActionResult> Index()
        {
            string currentUser = this.User.Identity.Name;
            var hrm = await _context.Hrms
                .Include(o => o.User)
                .Where(o => o.CreatedBy == currentUser)
                .OrderBy(o => o.Status)
                .ThenBy(o => o.ApprovalStatus)
                .ThenByDescending(o => o.HrmId)
                .ToListAsync();

            return View(hrm);
        }

        // GET: Hrms/Details/5
        public async Task<IActionResult> Details(int? id, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;

            if (id == null || _context.Hrms == null)
            {
                return NotFound();
            }

            var hrm = await _context.Hrms
                .Include(h => h.User)
                .FirstOrDefaultAsync(m => m.HrmId == id);
            if (hrm == null)
            {
                return NotFound();
            }

            return View(hrm);
        }

        // Get Customers
        [HttpGet]
        [ValidateAntiForgeryToken]
        public JsonResult GetCustomers(string department)
        {
            var customersList = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)")
                .Where(o => o.Department == department)
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct()
                .ToList();

            if (department == "PL-TCZ-PCBA-Supply Chain")
            {
                customersList.Add("COMMON");
            }

            return Json(customersList);
        }

        // Get Reasons
        [HttpGet]
        [ValidateAntiForgeryToken]
        public JsonResult GetReasons(string category)
        {
            var reason = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')")
                .Where(o => o.Category == category)
                .OrderBy(o => o.CorporateReason)
                .Select(o => new { o.CorporateReason, o.ReasonType })
                .Distinct()
                .ToList();

            return Json(reason);
        }

        // GET: Hrms/Create
        public IActionResult Create(int? DowntimeId, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;

            string currentUser = this.User.Identity.Name;
            var departments = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");

            var categories = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");
            //List<Tuple<string, string>> departmentsRaptor = new List<Tuple<string, string>>();
            //var elements = _context.InfoRecords.Where(o => o.IsVisible == true).Select(o => new { o.Category, o.CorporateReason }).ToList();
            //foreach (var elem in elements)
            //{
            //    departmentsRaptor.Add(new Tuple<string, string>(elem.Category, elem.CorporateReason));
            //}

            ViewData["UserId"] = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;
            ViewData["CreatedBy"] = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.NameSurname;
            ViewData["DowntimeId"] = DowntimeId;

            ViewData["Departments"] = departments?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct().ToList();

            ViewData["Customers"] = departments?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct().ToList();

            ViewData["Categories"] = categories?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct().ToList();

            ViewData["Reasons"] = categories?
                .OrderBy(o => o.CorporateReason)
                .Select(o => o.CorporateReason)
                .Distinct().ToList();


            return View();
        }

        // POST: Hrms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? DowntimeId, string sortOrder, int? pageNumber, [Bind("HrmId,Year,Week,Department,Customer,Category,Reason,StartTime,EndTime,EmployeeQty," +
            "TotalHours,Commentary,IsUA,Status,CreatedBy,CreatedAt,ClosedById,ClosedAt,Approver,ApprovalStatus,UserId,DowntimeId")] Hrm hrm)
        {
            string currentUser = this.User.Identity.Name;
            hrm.Year = hrm.StartTime.AddHours(-6.0).Year;
            hrm.Week = CultureInfo.CurrentCulture.Calendar
                .GetWeekOfYear(Convert.ToDateTime(hrm.StartTime.AddHours(-6.0)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);
            hrm.TotalHours = !string.IsNullOrEmpty(hrm.EndTime.ToString()) ? Convert.ToDecimal(((hrm.EndTime.GetValueOrDefault() - hrm.StartTime) * hrm.EmployeeQty).TotalHours.ToString("f2")) : Convert.ToDecimal(0.ToString("f2"));
            hrm.Approver = null;
            hrm.CreatedBy = currentUser;
            hrm.CreatedAt = DateTime.Now;
            hrm.UserId = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;

            var departments = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");

            var categories = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");

            ViewData["UserId"] = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;
            ViewData["CreatedBy"] = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.NameSurname;
            ViewData["DowntimeId"] = DowntimeId;

            ViewData["Departments"] = departments?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct().ToList();

            ViewData["Customers"] = departments?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct().ToList();

            ViewData["Categories"] = categories?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct().ToList();

            ViewData["Reasons"] = new SelectList(categories?
                .OrderBy(o => o.CorporateReason)
                .Select(o => new SelectListItem { Value = o.CorporateReason, Text = o.CorporateReason + " (" + o.ReasonType + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            ModelState.ClearValidationState("DowntimeId");
            ModelState.MarkFieldValid("DowntimeId");

            if (ModelState.IsValid)
            {
                _context.Add(hrm);
                await _context.SaveChangesAsync();
                _toasts.Success("Wolne zasoby zostały dodane", 5);
                return RedirectToAction("Index", "Hrms",
                    new { sortOrder = sortOrder, pageNumber = pageNumber });

            }
            else
            {
                _toasts.Error("Wolne zasoby nie zostały dodane", 5);
                ViewData["HrmId"] = new SelectList(_context.Hrms, "AssortmentGroupId", "HrmId", hrm.HrmId);
                return RedirectToAction("Create", "Hrms",
                    new { sortOrder = sortOrder, pageNumber = pageNumber });
            }
        }

        // GET: Hrms/Edit/5
        public async Task<IActionResult> Edit(int? id, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;

            if (id == null || _context.Hrms == null)
            {
                return NotFound();
            }

            var hrm = await _context.Hrms.FindAsync(id);
            if (hrm == null)
            {
                return NotFound();
            }

            var departments = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");

            var categories = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");

            ViewData["Departments"] = departments?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct().ToList();

            ViewData["Customers"] = departments?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct().ToList();

            ViewData["Categories"] = categories?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct().ToList();

            ViewData["Reasons"] = new SelectList(categories?
                .OrderBy(o => o.CorporateReason)
                .Select(o => new SelectListItem { Value = o.CorporateReason, Text = o.CorporateReason + " (" + o.ReasonType + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            return View(hrm);
        }

        // POST: Hrms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, int? DowntimeId, string sortOrder, int? pageNumber, [Bind("HrmId,Year,Week,Department,Customer,Category,Reason,StartTime,EndTime,EmployeeQty," +
            "TotalHours,Commentary,IsUA,Status,CreatedBy,CreatedAt,ClosedById,ClosedAt,Approver,ApprovalStatus,UserId,DowntimeId")] Hrm hrm)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;

            var hrms = _context.Hrms?.AsNoTracking().FirstOrDefault(o => o.HrmId == id);

            var departments = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");

            var categories = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");

            ViewData["Departments"] = departments?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct().ToList();

            ViewData["Customers"] = departments?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct().ToList();

            ViewData["Categories"] = categories?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct().ToList();

            ViewData["Reasons"] = new SelectList(categories?
                .OrderBy(o => o.CorporateReason)
                .Select(o => new SelectListItem { Value = o.CorporateReason, Text = o.CorporateReason + " (" + o.ReasonType + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            ViewData["DowntimeId"] = DowntimeId;

            if (id != hrm.HrmId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    hrm.TotalHours = !string.IsNullOrEmpty(hrm.EndTime.ToString()) ? Convert.ToDecimal(((hrm.EndTime.GetValueOrDefault() - hrm.StartTime) * hrm.EmployeeQty).TotalHours.ToString("f2")) : Convert.ToDecimal(0.ToString("f2"));
                    _context.Update(hrm);
                    await _context.SaveChangesAsync();
                    _toasts.Success("Zmiany zostały zapisane", 5);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HrmExists(hrm.HrmId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _toasts.Error("Wystąpił błąd! Zmiany nie zostały zapisane", 5);
                return RedirectToAction("Edit", "Hrms",
                    new { id = id, sortOrder = sortOrder, pageNumber = pageNumber });
            }
        }

        // GET: Hrms/Delete/5
        public async Task<IActionResult> Delete(int? id, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;

            if (id == null || _context.Hrms == null)
            {
                return NotFound();
            }

            var hrm = await _context.Hrms
                .Include(h => h.User)
                .FirstOrDefaultAsync(m => m.HrmId == id);
            if (hrm == null)
            {
                return NotFound();
            }

            return View(hrm);
        }

        // POST: Hrms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;

            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }
            else
            {
                var hrm = await _context.Hrms.FindAsync(id);
                if (hrm != null)
                {
                    _context.Hrms.Remove(hrm);
                    await _context.SaveChangesAsync();
                    _toasts.Success("Wolne zasoby zotały usunięte", 5);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _toasts.Error("Wystąpił błąd! Wolne zasoby nie zostały usunięte", 5);
                    return RedirectToAction("Delete", new { id = id });
                }
            }
        }

        // GET: Hrms/CloseRecord/5
        public async Task<IActionResult> CloseRecord(int id)
        {
            string currentUser = this.User.Identity.Name;
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var hrm = await _context.Hrms.FirstOrDefaultAsync(o => o.HrmId == id);
            if (hrm == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid || hrm.EndTime == null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                hrm.Status = 1;
                hrm.ClosedById = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;
                hrm.ClosedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                _toasts.Success("Wolne zasoby zostały przesłane do akceptacji", 5);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Hrms/Market
        public async Task<IActionResult> Market()
        {
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var currentTime = DateTime.Now;
            var hrm = await _context.Hrms
                .Include(o => o.User)
                .Where(o => o.EndTime > currentTime || o.EndTime == null && o.Status != 2)
                .OrderByDescending(o => o.EndTime)
                .ToListAsync();

            return View(hrm);

        }

        // GET: Hrms/Duplicate/5
        public async Task<IActionResult> Duplicate(int? id, Hrm hrm)
        {
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var hrm_duplicate = await _context.Hrms
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.HrmId == id);

            if (hrm_duplicate == null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditDuplicate", new { id = hrm_duplicate.HrmId, old_id = id });
        }


        // GET: Hrms/EditDuplicate/5/1
        [Route("Hrms/EditDuplicate/{id:int}/{old_id:int}")]
        public async Task<IActionResult> EditDuplicate(int? id, int? old_id)
        {
            if (_context.Hrms == null)
            {
                return NotFound();
            }

            var hrm = _context.Hrms
                .FirstOrDefaultAsync(o => o.HrmId == id);

            if (hrm == null)
            {
                return NotFound();
            }

            var departments = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");

            var categories = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");

            ViewData["Departments"] = departments?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct().ToList();

            ViewData["Customers"] = departments?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct().ToList();

            ViewData["Categories"] = categories?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct().ToList();

            ViewData["Reasons"] = categories?
                .OrderBy(o => o.CorporateReason)
                .Select(o => o.CorporateReason)
                .Distinct().ToList();

            return View(await hrm);
        }


        // GET: Hrms/EditDuplicate/5/1
        [HttpPost]
        [Route("Hrms/EditDuplicate/{id:int}/{old_id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDuplicate(int? id, int? old_id, Hrm hrm)
        {
            string currentUser = this.User.Identity.Name;
            Hrm hrm_old = _context.Hrms
                .FirstOrDefault(m => m.HrmId == old_id);

            hrm_old.EmployeeQty -= hrm.EmployeeQty;

            if (string.IsNullOrEmpty(hrm_old.EndTime.ToString()))
            {
                hrm_old.TotalHours = 0M;
            }
            else
            {
                hrm_old.TotalHours = Convert.ToDecimal(((hrm_old.EndTime.GetValueOrDefault() - hrm_old.StartTime) * hrm_old.EmployeeQty).TotalHours.ToString("f2"));
            }

            hrm.HrmId = 0;
            hrm.Year = hrm_old.Year;
            hrm.Week = hrm_old.Week;
            hrm.Department = hrm_old.Department;
            hrm.Customer = hrm_old.Customer;
            hrm.Category = hrm_old.Category;
            hrm.Reason = hrm_old.Reason;
            hrm.CreatedBy = hrm_old.CreatedBy;
            hrm.CreatedAt = hrm_old.CreatedAt;
            hrm.UserId = hrm_old.UserId;
            hrm.User = hrm_old.User;
            hrm.Approver = null;
            hrm.ApprovalStatus = hrm_old.ApprovalStatus;
            hrm.ClosedById = _context?.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;
            hrm.ClosedAt = new DateTime?(DateTime.Now);
            hrm.Status = 2;
            hrm.TotalHours = !string.IsNullOrEmpty(hrm.EndTime.ToString()) ?
                Convert.ToDecimal(((hrm.EndTime.GetValueOrDefault() - hrm.StartTime) * hrm.EmployeeQty).TotalHours.ToString("f2")) :
                Convert.ToDecimal(0.ToString("f2"));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(hrm);
                    await _context.SaveChangesAsync();
                    _toasts.Success("Wolne zasoby zostały podzielone", 5);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HrmExists(hrm.HrmId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            _toasts.Error("Wystąpił błąd! Wolne zasoby nie zostały podzielone", 5);
            return RedirectToAction("EditDuplicate", "Hrms", new { id = id, old_id = hrm.HrmId });
        }

        private bool HrmExists(int id)
        {
            return (_context.Hrms?.Any(e => e.HrmId == id)).GetValueOrDefault();
        }
    }
}