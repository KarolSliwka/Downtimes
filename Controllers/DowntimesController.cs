using AspNetCoreHero.ToastNotification.Abstractions;
using DowntimeTracker.Data;
using DowntimeTracker.Models;
using DowntimeTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DowntimeTracker.Controllers
{
    [Authorize(Policy = "AllUsers")]
    public class DowntimesController : Controller
    {
        private readonly TCZNT5000 _context;
        private readonly TCZNT5000Raptor _raptor;
        private readonly INotyfService _toasts;
        private readonly IUserService _userService;

        public DowntimesController(TCZNT5000 context, TCZNT5000Raptor raptor, INotyfService toast, IUserService userService)
        {
            _context = context;
            _raptor = raptor;
            _toasts = toast;
            _userService = userService;
            _userService = userService;
        }

        private string CurrentUserName
        {
            get
            {
                return User.Identity.Name.Substring(7);
            }
        }

        // GET: Downtimes
        public async Task<IActionResult> Index(int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;
            SetSortOrderViewBags(sortOrder);

            var downtimes = GetFilteredDowntimes(year_from, year_to, week_from, week_to, department, userId, claimed);

            SetViewData(year_from, year_to, week_from, week_to, department, userId, claimed, downtimes);

            downtimes = SortDowntimes(downtimes, sortOrder);

            int pageSize = 40;
            return View(await PaginatedList<Downtime>.CreateAsync(downtimes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        private void SetSortOrderViewBags(string sortOrder)
        {
            ViewBag.YearSort = sortOrder == "year" ? "year_desc" : "year";
            ViewBag.DateSort = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.PrimarySort = sortOrder == "primary" ? "primary_desc" : "primary";
            ViewBag.AreaSort = sortOrder == "area" ? "area_desc" : "area";
        }

        private IQueryable<Downtime> GetFilteredDowntimes(int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed)
        {
            var downtimes = _context.Downtimes.Include(d => d.MachineLineArea).Include(d => d.User)
                .Where(o => (year_from.HasValue && o.Year > year_from || (o.Year == year_from && o.Week >= week_from)) &&
                            (year_to.HasValue && o.Year < year_to || (o.Year == year_to && o.Week <= week_to)) &&
                            (string.IsNullOrEmpty(department) || o.Department.Contains(department)));

            if (userId.HasValue && userId > 0)
            {
                downtimes = downtimes.Where(o => o.UserId == userId);
            }

            if (!string.IsNullOrEmpty(claimed))
            {
                bool isClaimed = claimed == "true";
                downtimes = downtimes.Where(o => o.Claimed == isClaimed);
            }

            return downtimes;
        }

        private void SetViewData(int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, IQueryable<Downtime> downtimes)
        {
            ViewData["YearFrom"] = _context.Downtimes?
                .Select(o => o.Year)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            ViewData["YearTo"] = _context.Downtimes?
                .Where(o => o.Year >= year_from)
                .Select(o => o.Year)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            ViewData["WeekFrom"] = _context.Downtimes?
                .Where(o => o.Year == year_from)
                .Select(o => o.Week)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            ViewData["WeekTo"] = _context.Downtimes?
                .Where(o =>
                    (o.Year > year_from ||
                    (o.Year == year_from && (!week_from.HasValue || o.Week >= week_from))) &&
                    (o.Year == year_to))
                .Select(o => o.Week)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            ViewData["Departments"] = GetDepartments(year_from, year_to, week_from, week_to).ToList();
            ViewData["Creators"] = GetCreators(year_from, year_to, week_from, week_to, claimed, department).ToList();

            ViewData["FilterYearFrom"] = year_from;
            ViewData["FilterYearTo"] = year_to;
            ViewData["FilterWeekFrom"] = week_from;
            ViewData["FilterWeekTo"] = week_to;
            ViewData["FilterClaimed"] = claimed;
            ViewData["FilterDepartment"] = department;
            ViewData["FilterCreator"] = userId;

            ViewData["Years"] = new SelectList(new List<(int, int)>(), "Value", "Name");
        }

        private IQueryable<string> GetDepartments(int? year_from, int? year_to, int? week_from, int? week_to)
        {
            return _context.Downtimes?
                .Where(o => (!year_from.HasValue || o.Year > year_from || (o.Year == year_from && o.Week >= week_from)) &&
                            (!year_to.HasValue || o.Year < year_to || (o.Year == year_to && o.Week <= week_to)))
                .Select(o => o.Department)
                .Distinct()
                .OrderBy(o => o);
        }

        private SelectList GetCreators(int? year_from, int? year_to, int? week_from, int? week_to, string? claimed, string? department)
        {
            var creators = _context.Downtimes?
                .Include(o => o.User)
                .Where(o => (!year_from.HasValue || o.Year > year_from || (o.Year == year_from && o.Week >= week_from)) &&
                            (!year_to.HasValue || o.Year < year_to || (o.Year == year_to && o.Week <= week_to)) &&
                            (string.IsNullOrEmpty(claimed) || o.Claimed == (claimed == "true")) &&
                            (string.IsNullOrEmpty(department) || o.Department.Contains(department)))
                .Select(o => new { Value = o.UserId, Text = o.User.NameSurname })
                .Distinct()
                .OrderBy(o => o.Text)
                .ToList();

            return new SelectList(creators, "Value", "Text");
        }

        private IQueryable<Downtime> SortDowntimes(IQueryable<Downtime> downtimes, string sortOrder)
        {
            var newSortClass = "bi-sort-up-alt text-purple";
            var oldSortClass = "bi-sort-down-alt text-purple";
            var bSortClass = "bi-sort-down-alt";

            // Initialize all sort classes to the basic class
            ViewData["yearClass"] = bSortClass;
            ViewData["dateClass"] = bSortClass;
            ViewData["primaryClass"] = bSortClass;
            ViewData["areaClass"] = bSortClass;

            switch (sortOrder)
            {
                case "year":
                    downtimes = downtimes.OrderBy(o => o.Year).ThenBy(o => o.Week);
                    ViewData["yearClass"] = oldSortClass;
                    break;
                case "year_desc":
                    downtimes = downtimes.OrderByDescending(o => o.Year).ThenByDescending(o => o.Week);
                    ViewData["yearClass"] = newSortClass;
                    break;
                case "date":
                    downtimes = downtimes.OrderBy(o => o.EventStartTime).ThenBy(o => o.Year).ThenBy(o => o.Week);
                    ViewData["dateClass"] = oldSortClass;
                    break;
                case "date_desc":
                    downtimes = downtimes.OrderByDescending(o => o.EventStartTime).ThenByDescending(o => o.Year).ThenByDescending(o => o.Week);
                    ViewData["dateClass"] = newSortClass;
                    break;
                case "primary":
                    downtimes = downtimes.OrderBy(o => o.Department).ThenBy(o => o.Year).ThenBy(o => o.Week);
                    ViewData["primaryClass"] = oldSortClass;
                    break;
                case "primary_desc":
                    downtimes = downtimes.OrderByDescending(o => o.Department).ThenByDescending(o => o.Year).ThenByDescending(o => o.Week);
                    ViewData["primaryClass"] = newSortClass;
                    break;
                case "area":
                    downtimes = downtimes.OrderBy(o => o.MachineLineArea.Name).ThenBy(o => o.Year).ThenBy(o => o.Week);
                    ViewData["areaClass"] = oldSortClass;
                    break;
                case "area_desc":
                    downtimes = downtimes.OrderByDescending(o => o.MachineLineArea.Name).ThenByDescending(o => o.Year).ThenByDescending(o => o.Week);
                    ViewData["areaClass"] = newSortClass;
                    break;
                default:
                    downtimes = downtimes.OrderBy(o => o.Year).ThenBy(o => o.Week);
                    break;
            }

            return downtimes;
        }

        // GET: Weeks
        [HttpGet]
        [ValidateAntiForgeryToken]
        public JsonResult GetCookie(string machineCookie)
        {
            var operatingTimes = _context.MachineLineAreas?
                .Where(o => o.Name == machineCookie)
                .Select(o => o.OperatingTime)
                .Distinct();

            return Json(operatingTimes);
        }

        // GET: GetYearTo
        [HttpGet]
        public JsonResult GetYearTo(int year_from)
        {
            var years = _context.Downtimes?
                .Where(o => o.Year >= year_from)
                .Select(o => o.Year)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            return Json(years);
        }

        // GET: WeeksFromYear
        [HttpGet]
        public JsonResult GetWeeksFrom(int year_from)
        {
            var weeks = _context.Downtimes?
                .Where(o => o.Year == year_from)
                .Select(o => o.Week)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            return Json(weeks);
        }

        // GET: WeeksToYear
        [HttpGet]
        public JsonResult GetWeeksTo(int year_from, int year_to, int? week_from)
        {
            var weeks = _context.Downtimes?
                .Where(o =>
                    (o.Year > year_from ||
                    (o.Year == year_from && (!week_from.HasValue || o.Week >= week_from))) &&
                    (o.Year == year_to))
                .Select(o => o.Week)
                .Distinct()
                .OrderByDescending(o => o)
                .ToList();

            return Json(weeks);
        }

        // GET: PrimaryOrganization
        [HttpGet]
        public JsonResult GetPrimaries(int year_from, int year_to, int week_from, int week_to)
        {
            var departments = _context.Downtimes
                .Where(o => (o.Year > year_from || (o.Year == year_from && o.Week >= week_from)) &&
                            (o.Year < year_to || (o.Year == year_to && o.Week <= week_to)))
                .Select(o => o.Department)
                .Distinct()
                .OrderBy(o => o);

            return Json(departments);
        }

        // GET: Creators
        [HttpGet]
        public JsonResult GetCreatedBy(int year_from, int year_to, int week_from, int week_to, string? claimed, string department)
        {
            var creators = new SelectList(_context.Downtimes?
                .Include(o => o.User)
                .Where(o => (o.Year > year_from || (o.Year == year_from && o.Week >= week_from)) &&
                            (o.Year < year_to || (o.Year == year_to && o.Week <= week_to)) &&
                            (string.IsNullOrEmpty(claimed) || o.Claimed == (claimed == "true")) &&
                            (string.IsNullOrEmpty(department) || o.Department.Contains(department)))
                .Select(o => new { Value = o.UserId, Text = o.User.NameSurname })
                .Distinct()
                .OrderBy(o => o.Text)
                .ToList(), "Value", "Text");

            return Json(creators);
        }

        // GET: ClaimStatus
        [HttpGet]
        [ValidateAntiForgeryToken]
        public JsonResult GetStatus(string category, string reason)
        {
            var category_set = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");
            var claims = category_set?
                .Where(o => o.Category == category && o.CorporateReason == reason)
                .Select(o => o.IsClaimable)
                .ToList();

            return Json(claims);
        }

        // GET: Types
        [HttpGet]
        [ValidateAntiForgeryToken]
        public JsonResult GetTypes(string department)
        {
            List<Tuple<int, string>> typeList = new List<Tuple<int, string>>();

            var types = _context.MachineLineAreas?
                .Where(o => o.Department == department);

            var areas = types?.Where(o => o.Type == 0).Count();
            var machines = types?.Where(o => o.Type == 1).Count();
            var lines = types?.Where(o => o.Type == 2).Count();

            string areasInfo = "Obszar (" + areas + ")";
            string machinesInfo = "Maszyna (" + machines + ")";
            string linesInfo = "Linia (" + lines + ")";

            typeList.Add(new Tuple<int, string>(0, areasInfo));
            typeList.Add(new Tuple<int, string>(1, machinesInfo));
            typeList.Add(new Tuple<int, string>(2, linesInfo));

            return Json(typeList);
        }

        // GET: Downtimes/Details/5
        public async Task<IActionResult> Details(int? id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;
            ViewData["FilterYearFrom"] = year_from;
            ViewData["FilterYearTo"] = year_to;
            ViewData["FilterWeekFrom"] = week_from;
            ViewData["FilterWeekTo"] = week_to;
            ViewData["FilterClaimed"] = claimed;
            ViewData["FilterDepartment"] = department;
            ViewData["FilterCreator"] = userId;

            if (id == null || _context.Downtimes == null)
            {
                return NotFound();
            }

            var downtime = await _context.Downtimes
                .Include(o => o.User)
                .Include(o => o.MachineLineArea)
                .FirstOrDefaultAsync(m => m.DowntimeId == id);
            if (downtime == null)
            {
                return NotFound();
            }

            return View(downtime);
        }

        // GET: Downtimes/Create
        public async Task<IActionResult> Create()
        {
            string currentUser = this.User.Identity.Name;
            var primaries_set = _raptor.Departments?.FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");
            var category_set = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");
            var customers = primaries_set?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct()
                .ToList();

            var reasons = category_set?
                .OrderBy(o => o.CorporateReason)
                .Select(o => o.CorporateReason)
                .Distinct()
                .ToList();

            ViewData["Departments"] = primaries_set?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct()
                .ToList();

            ViewData["Categories"] = category_set?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct()
                .ToList();

            ViewData["Machines"] = new SelectList(_context.MachineLineAreas?.FromSqlRaw("SELECT * FROM MachineLineAreas WITH (NOLOCK)")
                .OrderBy(o => o.Name)
                .Select(o => new SelectListItem { Value = o.Name, Text = o.Name + " (" + o.OperatingTime + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            ViewData["Customers"] = "";
            ViewData["Reasons"] = "";
            ViewData["UserId"] = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;

            return View();
        }

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

        // GET Machines
        [HttpGet]
        [ValidateAntiForgeryToken]
        public JsonResult GetMachines(string department, int type)
        {
            var machines = new SelectList(_context.MachineLineAreas?
                .Where(o => o.Department == department && o.Type == type)
                .Select(o => new SelectListItem { Value = o.MachineLineAreaId.ToString(), Text = o.Name + " (" + o.OperatingTime + "h)" })
                .OrderBy(o => o.Text)
                .ToList(), "Value", "Text")
                .Distinct();

            return Json(machines);
        }

        // POST: Downtimes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DowntimeId,Year,Week,Site,EventStartTime,EventEndTime,Department,Customer,Category," +
            "Reason,PeopleAffected,TotalHours,IsClaimable,Commentary,MachineLineAreaId,ApproverEmailADID,CreatedBy,UserId,CreationDate,Claimed,ClaimedBy,ClaimedAt")] Downtime downtime)
        {
            string currentUser = this.User.Identity.Name;
            // overide empty fields with data
            downtime.Site = "PL-TCZ-PCBA";
            downtime.Year = downtime.EventStartTime.Year;
            //downtime.Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(Convert.ToDateTime(downtime.EventStartTime.AddHours(-6)), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Saturday);
            downtime.Week = GetIso8601WeekOfYear(downtime.EventStartTime);
            downtime.TotalHours = Convert.ToDecimal(((downtime.EventEndTime - downtime.EventStartTime) * downtime.PeopleAffected).TotalHours.ToString("f2"));
            downtime.CreatedBy = currentUser;
            downtime.CreationDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(downtime);
                await _context.SaveChangesAsync();
                _toasts.Success("Przestój został dodany", 5);
                return RedirectToAction(nameof(Create));
            }
            else
            {
                _toasts.Error("Przestój nie został dodany", 5);
            }

            var primaries_set = _raptor.Departments?.FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");
            var category_set = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");
            var customers = primaries_set?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct()
                .ToList();

            var reasons = category_set?
                .OrderBy(o => o.CorporateReason)
                .Select(o => o.CorporateReason)
                .Distinct()
                .ToList();

            ViewData["Departments"] = primaries_set?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct()
                .ToList();

            ViewData["Categories"] = category_set?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct()
                .ToList();

            ViewData["Machines"] = new SelectList(_context.MachineLineAreas?.FromSqlRaw("SELECT * FROM MachineLineAreas WITH (NOLOCK)")
                .OrderBy(o => o.Name)
                .Select(o => new SelectListItem { Value = o.Name, Text = o.Name + " (" + o.OperatingTime + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            ViewData["Customers"] = "";
            ViewData["Reasons"] = "";
            ViewData["UserId"] = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.UserId;

            return View(downtime);
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Adjust the time to start the week on Saturday at 06:00:00
            DateTime adjustedTime = time.AddHours(-6);
            DayOfWeek day = adjustedTime.DayOfWeek;

            // ISO 8601 week starts on Monday, so we need to adjust accordingly
            if (day >= DayOfWeek.Saturday)
            {
                adjustedTime = adjustedTime.AddDays(8 - (int)day);
            }
            else
            {
                adjustedTime = adjustedTime.AddDays(1 - (int)day);
            }

            // Use ISO 8601 week numbering
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule rule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            return calendar.GetWeekOfYear(adjustedTime, rule, firstDayOfWeek);
        }

        // GET: Downtimes/Edit/5
        public async Task<IActionResult> Edit(int? id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;
            ViewData["FilterYearFrom"] = year_from;
            ViewData["FilterYearTo"] = year_to;
            ViewData["FilterWeekFrom"] = week_from;
            ViewData["FilterWeekTo"] = week_to;
            ViewData["FilterClaimed"] = claimed;
            ViewData["FilterDepartment"] = department;
            ViewData["FilterCreator"] = userId;

            var primaries_set = _raptor.Departments?.FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");
            var category_set = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");
            var customers = primaries_set?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct()
                .ToList();

            var reasons = category_set?
                .OrderBy(o => o.CorporateReason)
                .Select(o => o.CorporateReason)
                .Distinct()
                .ToList();

            ViewData["Departments"] = primaries_set?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct()
                .ToList();

            ViewData["Categories"] = category_set?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct()
                .ToList();

            ViewData["Machines"] = new SelectList(_context.MachineLineAreas?.FromSqlRaw("SELECT * FROM MachineLineAreas WITH (NOLOCK)")
                .OrderBy(o => o.Name)
                .Select(o => new SelectListItem { Value = o.Name, Text = o.Name + " (" + o.OperatingTime + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            ViewData["Customers"] = "";
            ViewData["Reasons"] = "";

            if (id == null || _context.Downtimes == null)
            {
                return NotFound();
            }

            var downtime = await _context.Downtimes.FindAsync(id);
            if (downtime == null)
            {
                return NotFound();
            }
            return View(downtime);
        }

        // POST: Downtimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber, [Bind("DowntimeId,Year,Week,Site,EventStartTime,EventEndTime,Department,Customer,Category," +
            "Reason,PeopleAffected,TotalHours,IsClaimable,Commentary,MachineLineArea,ApproverEmailADID,CreatedBy,UserId,CreationDate,Claimed,ClaimedBy,ClaimedAt")] Downtime downtime)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;
            ViewData["FilterYearFrom"] = year_from;
            ViewData["FilterYearTo"] = year_to;
            ViewData["FilterWeekFrom"] = week_from;
            ViewData["FilterWeekTo"] = week_to;
            ViewData["FilterClaimed"] = claimed;
            ViewData["FilterDepartment"] = department;
            ViewData["FilterCreator"] = userId;

            var primaries_set = _raptor.Departments?.FromSqlRaw("SELECT * FROM DepartmentMappings WITH (NOLOCK)");
            var category_set = _raptor.Categories?.FromSqlRaw("SELECT * FROM CategoryReasonMapping WITH (NOLOCK) " +
                "WHERE CorporateReason NOT IN('C01-Holiday Shutdown', 'P04-Rest Breaks', 'P05-Meal Breaks', 'P15-Vacation Shut Down', 'P17-Holiday Shutdown')");
            var customers = primaries_set?
                .OrderBy(o => o.Customer)
                .Select(o => o.Customer)
                .Distinct()
                .ToList();

            var reasons = category_set?
                .OrderBy(o => o.CorporateReason)
                .Select(o => o.CorporateReason)
                .Distinct()
                .ToList();

            ViewData["Departments"] = primaries_set?
                .OrderBy(o => o.Department)
                .Select(o => o.Department)
                .Distinct()
                .ToList();

            ViewData["Categories"] = category_set?
                .OrderBy(o => o.Category)
                .Select(o => o.Category)
                .Distinct()
                .ToList();

            ViewData["Machines"] = new SelectList(_context.MachineLineAreas?.FromSqlRaw("SELECT * FROM MachineLineAreas WITH (NOLOCK)")
                .OrderBy(o => o.Name)
                .Select(o => new SelectListItem { Value = o.Name, Text = o.Name + " (" + o.OperatingTime + ")" })
                .ToList(), "Value", "Text")
                .Distinct();

            ViewData["Customers"] = "";
            ViewData["Reasons"] = "";

            if (id != downtime.DowntimeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(downtime);
                    await _context.SaveChangesAsync();
                    _toasts.Success("Zmiany zostały zapisane", 5);
                    return RedirectToAction("Index", "Downtimes",
                        new { year_from = year_from, year_to = year_to, week_from = week_from, week_to = week_to, department = department, userId = userId, claimed = claimed, sortOrder = sortOrder, pageNumber = pageNumber });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DowntimeExists(downtime.DowntimeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                _toasts.Error("Wystąpił błąd! Zmiany nie zostały zapisane", 5);
                return RedirectToAction("Edit", "Downtimes",
                    new { id = id, year_from = year_from, year_to = year_to, week_from = week_from, week_to = week_to, department = department, userId = userId, claimed = claimed, sortOrder = sortOrder, pageNumber = pageNumber });
            }
        }

        // GET: Downtimes/Delete/5
        public async Task<IActionResult> Delete(int? id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;
            ViewData["FilterYearFrom"] = year_from;
            ViewData["FilterYearTo"] = year_to;
            ViewData["FilterWeekFrom"] = week_from;
            ViewData["FilterWeekTo"] = week_to;
            ViewData["FilterClaimed"] = claimed;
            ViewData["FilterDepartment"] = department;
            ViewData["FilterCreator"] = userId;

            if (id == null || _context.Downtimes == null)
            {
                return NotFound();
            }

            var downtime = await _context.Downtimes
                .FirstOrDefaultAsync(m => m.DowntimeId == id);
            if (downtime == null)
            {
                return NotFound();
            }

            return View(downtime);
        }

        // POST: Downtimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            if (_context.Downtimes == null)
            {
                return Problem("Entity set 'TCZNT5000.Downtimes'  is null.");
            }
            else
            {
                var downtime = await _context.Downtimes.FindAsync(id);
                if (downtime != null)
                {
                    _context.Downtimes.Remove(downtime);
                    await _context.SaveChangesAsync();
                    _toasts.Success("Przestój zotał usunięty", 5);
                    return RedirectToAction("Index", "Downtimes",
                        new { year_from = year_from, year_to = year_to, week_from = week_from, week_to = week_to, department = department, userId = userId, claimed = claimed, sortOrder = sortOrder, pageNumber = pageNumber });
                }
                else
                {
                    _toasts.Error("Wystąpił błąd! Przestój nie został usunięty", 5);
                    return RedirectToAction("Index", "Downtimes",
                        new { year_from = year_from, year_to = year_to, week_from = week_from, week_to = week_to, department = department, userId = userId, claimed = claimed, sortOrder = sortOrder, pageNumber = pageNumber });
                }
            }
        }

        // GET: Downtimes/Claimed/5
        public async Task<IActionResult> Claim(int? id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["pageNumber"] = pageNumber;
            ViewData["FilterYearFrom"] = year_from;
            ViewData["FilterYearTo"] = year_to;
            ViewData["FilterWeekFrom"] = week_from;
            ViewData["FilterWeekTo"] = week_to;
            ViewData["FilterClaimed"] = claimed;
            ViewData["FilterDepartment"] = department;
            ViewData["FilterCreator"] = userId;

            if (id == null || _context.Downtimes == null)
            {
                return NotFound();
            }

            var downtime = await _context.Downtimes
                .Include(o => o.User)
                .Include(o => o.MachineLineArea)
                .FirstOrDefaultAsync(m => m.DowntimeId == id);
            if (downtime == null)
            {
                return NotFound();
            }

            return View(downtime);
        }

        // POST: Downtimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(int id, int? year_from, int? year_to, int? week_from, int? week_to, string? department, int? userId, string? claimed, string sortOrder, int? pageNumber)
        {
            string currentUser = this.User.Identity.Name;
            var downtime = await _context.Downtimes
                .FirstOrDefaultAsync(m => m.DowntimeId == id);

            downtime.Claimed = true;
            downtime.ClaimedBy = _context.Users?.FirstOrDefault(o => o.UserAd == currentUser)?.NameSurname;
            downtime.ClaimedAt = DateTime.Now;
            _context.Update(downtime);
            await _context.SaveChangesAsync();
            _toasts.Success("Roszczenie zostało zapisne", 5);
            return RedirectToAction("Index", "Downtimes",
                new { year_from = year_from, year_to = year_to, week_from = week_from, week_to = week_to, department = department, userId = userId, claimed = claimed, sortOrder = sortOrder, pageNumber = pageNumber });
        }

        private bool DowntimeExists(int id)
        {
            return (_context.Downtimes?.Any(e => e.DowntimeId == id)).GetValueOrDefault();
        }
    }
}
