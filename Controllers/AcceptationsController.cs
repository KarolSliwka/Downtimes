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
    public class AcceptationsController : Controller
    {
        private readonly TCZNT5000 _context;
        private readonly TCZNT5000Raptor _raptor;
        private readonly INotyfService _toasts;

        public AcceptationsController(TCZNT5000 context, TCZNT5000Raptor raptor, INotyfService toast)
        {
            _context = context;
            _raptor = raptor;
            _toasts = toast;
        }

        // Get: Acceptations
        public async Task<IActionResult> Index(int? pageNumber)
        {
            string currentUser = this.User.Identity.Name;
            var user = _context?.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.AccessLevel;
            ViewData["x"] = CultureInfo.CurrentCulture.Calendar
                .GetWeekOfYear(Convert.ToDateTime(DateTime.Now.AddHours(-6.0)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);

            var subordinates = _context?.Personel?
                .Where(o => o.SupAd == currentUser.Substring(7))
                .Select(o => o.EmpAd)
                .ToList();

            var list = _context?.Personel?
                .Where(o => subordinates
                .Contains(o.SupAd))
                .Select(o => o.EmpAd)
                .ToList();

            subordinates.AddRange(list);
            subordinates.Add(currentUser.Substring(7));

            var listAsync = _context.Hrms.Where(o => o.HrmId >= 0 && o.HrmId <= 10);
            int pageSize = 40;

            if (user == "super")
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1)
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2)
                    .Count();
            }
            else
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();
            }

            if (user == "super")
            {
                if (_context?.Hrms != null)
                {
                    listAsync = _context.Hrms
                        .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0)
                        .Include(o => o.User)
                        .OrderByDescending(o => o.HrmId);
                }
                else
                {
                    return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
                }
                return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));

            }

            if (_context?.Hrms != null)
            {
                listAsync = _context.Hrms
                    .Include(o => o.User)
                    .Where(o => subordinates.Contains(o.CreatedBy.Substring(7)) && (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0)
                    .OrderByDescending(o => o.HrmId);
            }
            else
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            ViewData["CurrentUser"] = currentUser;
            return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));

        }

        // GET: Applications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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

            var categories = _raptor.Categories?
                .FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK)");

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

        // POST: Applications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int? DowntimeId, int? pageNumber, [Bind("HrmId,Year,Week,Department,Customer,Category,Reason,StartTime,EndTime,EmployeeQty," +
            "TotalHours,Commentary,IsUA,Status,CreatedBy,CreatedAt,ClosedById,ClosedAt,Approver,ApprovalStatus,UserId,DowntimeId")] Hrm hrm)
        {
            var hrms = _context.Hrms?.AsNoTracking().FirstOrDefault(o => o.HrmId == id);

            var departments = _raptor.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");

            var categories = _raptor.Categories?
                .FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK)");

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
                return RedirectToAction(nameof(Opened));
            }
            else
            {
                _toasts.Error("Wystąpił błąd! Zmiany nie zostały zapisane", 5);
                return RedirectToAction("Edit", "Hrms",
                    new { id = id, pageNumber = pageNumber });
            }
        }

        // Get: Acceptations/Opened
        public async Task<IActionResult> Opened(int? pageNumber)
        {
            string currentUser = this.User.Identity.Name;
            var user = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.AccessLevel;

            ViewData["x"] = CultureInfo.CurrentCulture.Calendar
                .GetWeekOfYear(Convert.ToDateTime(DateTime.Now.AddHours(-6.0)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);

            var subordinates = _context.Personel?
                .Where(o => o.SupAd == currentUser.Substring(7))
                .Select(o => o.EmpAd)
                .ToList();

            var list = _context.Personel?
                .Where(o => subordinates
                .Contains(o.SupAd))
                .Select(o => o.EmpAd)
                .ToList();

            subordinates.AddRange(list);
            subordinates.Add(currentUser.Substring(7));

            var listAsync = _context.Hrms.Where(o => o.HrmId >= 0 && o.HrmId <= 10);
            int pageSize = 40;

            if (user == "super")
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1)
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2)
                    .Count();
            }
            else
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();
            }

            if (user == "super")
            {
                if (_context.Hrms != null)
                {
                    listAsync = _context.Hrms
                        .Where(o => o.Status == 0)
                        .Include(o => o.User)
                        .OrderBy(o => o.Status)
                        .ThenByDescending(o => o.HrmId);
                }
                else
                {
                    return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
                }
                return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            if (_context.Hrms != null)
            {
                listAsync = _context.Hrms
                    .Include(o => o.User)
                    .Where(o => subordinates.Contains(o.CreatedBy.Substring(7)) && o.Status == 0)
                    .OrderByDescending(o => o.HrmId);
            }
            else
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }
            return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // Get: Acceptations/Approved
        public async Task<IActionResult> Accepted(int? pageNumber)
        {
            string currentUser = this.User.Identity.Name;
            var user = _context.Users.FirstOrDefault(o => o.UserAd == currentUser)?.AccessLevel;

            ViewData["x"] = CultureInfo.CurrentCulture.Calendar
                .GetWeekOfYear(Convert.ToDateTime(DateTime.Now.AddHours(-6.0)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);

            var subordinates = _context.Personel?
                .Where(o => o.SupAd == currentUser.Substring(7))
                .Select(o => o.EmpAd)
                .ToList();

            var list = _context.Personel?
                .Where(o => subordinates
                .Contains(o.SupAd))
                .Select(o => o.EmpAd)
                .ToList();

            subordinates.AddRange(list);
            subordinates.Add(currentUser.Substring(7));

            var listAsync = _context.Hrms.Where(o => o.HrmId >= 0 && o.HrmId <= 10);
            int pageSize = 40;

            if (user == "super")
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1)
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2)
                    .Count();
            }
            else
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();
            }

            if (user == "super")
            {
                if (_context.Hrms != null)
                {
                    listAsync = _context.Hrms
                        .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1)
                        .Include(o => o.User)
                        .OrderByDescending(o => o.HrmId);
                }
                else
                {
                    return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
                }
                return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            if (_context.Hrms != null)
            {
                listAsync = _context.Hrms
                    .Include(o => o.User)
                    .Where(o => subordinates.Contains(o.CreatedBy.Substring(7)) && (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1)
                    .OrderByDescending(o => o.HrmId);

                View(listAsync);
            }
            else
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // Get: Acceptations/Refused
        public async Task<IActionResult> Refused(int? pageNumber)
        {
            string currentUser = this.User.Identity.Name;
            var user = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.AccessLevel;

            ViewData["x"] = CultureInfo.CurrentCulture.Calendar
                .GetWeekOfYear(Convert.ToDateTime(DateTime.Now.AddHours(-6.0)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);

            var subordinates = _context.Personel?
                .Where(o => o.SupAd == currentUser.Substring(7))
                .Select(o => o.EmpAd)
                .ToList();

            var list = _context.Personel?
                .Where(o => subordinates
                .Contains(o.SupAd))
                .Select(o => o.EmpAd)
                .ToList();

            subordinates.AddRange(list);
            subordinates.Add(currentUser.Substring(7));

            var listAsync = _context.Hrms.Where(o => o.HrmId >= 0 && o.HrmId <= 10);
            int pageSize = 40;

            if (user == "super")
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0)
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1)
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2)
                    .Count();
            }
            else
            {
                ViewData["OpenedCounter"] = _context.Hrms
                    .Where(o => o.Status == 0 && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AwaitingCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 0 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["AcceptedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 1 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();

                ViewData["RefusedCounter"] = _context.Hrms
                    .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2 && subordinates.Contains(o.CreatedBy.Substring(7)))
                    .Count();
            }

            if (user == "super")
            {
                if (_context.Hrms != null)
                {
                    listAsync = _context.Hrms
                        .Where(o => (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2)
                        .Include(o => o.User)
                        .OrderByDescending(o => o.HrmId);
                }
                else
                {
                    return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
                }
                return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));

            }

            if (_context.Hrms != null)
            {
                listAsync = _context.Hrms
                    .Include(o => o.User)
                    .Where(o => subordinates.Contains(o.CreatedBy.Substring(7)) && (o.Status == 1 || o.Status == 2) && o.ApprovalStatus == 2)
                    .OrderByDescending(o => o.HrmId);

                View(listAsync);
            }
            else
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            return View(await PaginatedList<Hrm>.CreateAsync(listAsync.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Acceptations/Details/5
        public async Task<IActionResult> Details(int? id, int page)
        {
            if (id == null || _context.Hrms == null)
            {
                return NotFound();
            }

            var model = await _context.Hrms
                .Include(h => h.User)
                .FirstOrDefaultAsync(m => m.HrmId == id);
            if (model == null)
            {
                return NotFound();
            }

            string pageName = "Index";
            switch (page)
            {
                case 0:
                    pageName = "Opened";
                    break;
                case 1:
                    pageName = "Index";
                    break;
                case 2:
                    pageName = "Accepted";
                    break;
                case 3:
                    pageName = "Refused";
                    break;
            }

            ViewData["Page"] = pageName;
            ViewData["ClosedBy"] = _context.Users?.FirstOrDefault(o => o.UserId == model.ClosedById)?.NameSurname;

            return View(model);
        }

        // Acceptations/RepoenRecord/5
        public async Task<IActionResult> ReopenRecord(int id)
        {
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var hrm = await _context.Hrms
                .FirstOrDefaultAsync(m => m.HrmId == id);

            if (hrm != null)
            {
                hrm.Status = 0;
                hrm.ApprovalStatus = 0;
                hrm.ClosedById = null;
                hrm.ClosedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                _toasts.Success("Wpis został cofnięty do poprawy", 5);
            }
            return RedirectToAction(nameof(Index));
        }

        // Acceptations/ApproveRecord/5
        public async Task<IActionResult> ApproveRecord(int id)
        {
            string currentUser = this.User.Identity.Name;
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var hrm = await _context.Hrms
                .FirstOrDefaultAsync(m => m.HrmId == id);

            if (hrm != null)
            {
                hrm.ApprovalStatus = 1;
                hrm.Approver = _context.Users?
                    .FirstOrDefault(o => o.UserAd == currentUser)?.NameSurname;
                await _context.SaveChangesAsync();
                _toasts.Success("Wolne zasoby zostały zaakceptowane", 5);
            }
            return RedirectToAction(nameof(Index));
        }

        // Acceptations/RefuseRecord/5
        public async Task<IActionResult> RefuseRecord(int id)
        {
            string currentUser = this.User.Identity.Name;
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var hrm = await _context.Hrms
                .FirstOrDefaultAsync(m => m.HrmId == id);

            if (hrm != null)
            {
                hrm.ApprovalStatus = 2;
                hrm.Approver = _context.Users?
                    .FirstOrDefault(o => o.UserAd == currentUser)?.NameSurname;
                await _context.SaveChangesAsync();
                _toasts.Success("Wolne zasoby zosatły odrzucone", 5);
            }
            return RedirectToAction(nameof(Index));
        }

        // Acceptations/CloseRecord/5
        public async Task<IActionResult> CloseRecord(int id)
        {
            string currentUser = this.User.Identity.Name;
            if (_context.Hrms == null)
            {
                return Problem("Entity set 'TCZNT5000.Hrms'  is null.");
            }

            var hrm = await _context.Hrms
                .FirstOrDefaultAsync(m => m.HrmId == id);

            if (hrm != null)
            {
                if (ModelState.IsValid || hrm.EndTime != null)
                {
                    hrm.Status = 1;
                    hrm.ClosedById = _context.Users?
                        .FirstOrDefault(o => o.UserAd == currentUser)?.UserId;
                    hrm.ClosedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    _toasts.Success("Wolne zasoby zostały przesłane do akceptacji", 5);
                }
                else
                {
                    return RedirectToAction("Details", new { id = hrm?.HrmId });
                }
            }
            return RedirectToAction("Opened");
        }

        // GET: Acceptations/Duplicate/5
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
                hrm_duplicate.HrmId = 0;
                hrm_duplicate.Commentary = "";
                _context.Add(hrm_duplicate);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditDuplicate", new { id = hrm_duplicate.HrmId, old_id = id });
        }


        // GET: Acceptations/EditDuplicate/5/1
        [Route("Acceptations/EditDuplicate/{id:int}/{old_id:int}")]
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

            var categories = _raptor.Categories?
                .FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK)");

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


        // GET: Acceptations/EditDuplicate/5/1
        [HttpPost]
        [Route("Acceptations/EditDuplicate/{id:int}/{old_id:int}")]
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

            hrm.Year = hrm_old.Year;
            hrm.Week = hrm_old.Week;
            hrm.Department = hrm_old.Department;
            hrm.Customer = hrm_old.Customer;
            hrm.Category = hrm_old.Category;
            hrm.Reason = hrm_old.Reason;
            hrm.StartTime = hrm_old.StartTime;
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
                    await _context.SaveChangesAsync();
                    _toasts.Success("Wolne zasoby zostały podzielone", 5);
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
                return RedirectToAction("Index");
            }
            return RedirectToAction("EditDuplicate", "Hrms", new { id = id, old_id = hrm.HrmId });
        }

        private bool HrmExists(int id)
        {
            return (_context.Hrms?.Any(e => e.HrmId == id)).GetValueOrDefault();
        }
    }
}