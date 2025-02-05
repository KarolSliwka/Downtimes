using AspNetCoreHero.ToastNotification.Abstractions;
using DowntimeTracker.Data;
using DowntimeTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DowntimeTracker.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class MachineLinesController : Controller
    {
        private readonly TCZNT5000 _context;
        private readonly TCZNT5000Raptor _raptor;
        private readonly INotyfService _toasts;
        private readonly object machines;

        public MachineLinesController(TCZNT5000 context, TCZNT5000Raptor raptor, INotyfService toast)
        {
            _context = context;
            _raptor = raptor;
            _toasts = toast;
            machines = new[]
            {
                new { Value = "0", Name = "Obszar" },
                new { Value = "1", Name = "Maszyna" },
                new { Value = "2", Name = "Linia" }
            }.ToList();
        }

        // GET: MachineLines
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var machines = _context.MachineLineAreas
                .OrderByDescending(o => o.Type)
                .ThenBy(o => o.OperatingTime)
                .ThenBy(o => o.Department)
                .ThenBy(o => o.Name);
            int pageSize = 20;
            return View(await PaginatedList<MachineLineArea>.CreateAsync(machines.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: MachineLines/Create
        public IActionResult Create()
        {
            ViewData["Machines"] = new SelectList((System.Collections.IEnumerable)machines, "Value", "Name");
            ViewData["Departments"] = _raptor.Departments
                .FromSqlRaw("SELECT * FROM DepartmentMappings")
                .Select(o => o.Department)
                .Distinct().ToList();

            return View();
        }

        // POST: MachineLines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MachineLineAreaId,Type,Department,Name,OperatingTime")] MachineLineArea machineLine)
        {
            ViewBag.Departments = _raptor?.Departments?.FromSqlRaw("SELECT * FROM DepartmentMappings").Select(o => o.Department).Distinct().ToList();

            var duplicate = _context.MachineLineAreas?
                .Where(o => o.Name.ToLower() == machineLine.Name.ToLower() && o.Type == machineLine.Type && o.Department == machineLine.Department)
                .ToList()
                .Count();

            ViewData["Machines"] = new SelectList((System.Collections.IEnumerable)machines, "Value", "Name");
            ViewData["Departments"] = _raptor?.Departments?
                .FromSqlRaw("SELECT * FROM DepartmentMappings")
                .Select(o => o.Department)
                .Distinct().ToList();

            if (ModelState.IsValid)
            {
                if (duplicate == 0)
                {
                    _context.Add(machineLine);
                    await _context.SaveChangesAsync();
                    if (machineLine.Type == 0)
                    {
                        _toasts.Success("Obszar został dodany", 5);
                    }
                    else if (machineLine.Type == 1)
                    {
                        _toasts.Success("Maszyna została dodana", 5);
                    }
                    else
                    {
                        _toasts.Success("Linia została dodana", 5);
                    }
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    if (machineLine.Type == 0)
                    {
                        _toasts.Custom("Obszar o wybranym Typie/Nazwie/Głównej Organizacji już istnieje", 5, "#f9b30f", "fa fa-warning text-white");
                    }
                    else if (machineLine.Type == 1)
                    {
                        _toasts.Custom("Maszyna o wybranym Typie/Nazwie/Głównej Organizacji już istnieje", 5, "#f9b30f", "fa fa-warning text-white");
                    }
                    else
                    {
                        _toasts.Custom("Linia o wybranym Typie/Nazwie/Głównej Organizacji już istnieje", 5, "#f9b30f", "fa fa-warning text-white");
                    }
                    return View(machineLine);

                }
            }
            else
            {
                _toasts.Error("Obszar/Maszyna/Linia nie została dodana", 5);
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: MachineLines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MachineLineAreas == null)
            {
                return NotFound();
            }

            var machineLine = await _context.MachineLineAreas.FindAsync(id);
            if (machineLine == null)
            {
                return NotFound();
            }

            ViewData["Machines"] = new SelectList((System.Collections.IEnumerable)machines, "Value", "Name");
            ViewData["Departments"] = _raptor.Departments
                .FromSqlRaw("SELECT * FROM DepartmentMappings")
                .Select(o => o.Department)
                .Distinct().ToList();

            return View(machineLine);
        }

        // POST: MachineLines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MachineLineAreaId,Type,Department,Name,OperatingTime")] MachineLineArea machineLine)
        {
            ViewData["Machines"] = new SelectList((System.Collections.IEnumerable)machines, "Value", "Name");
            ViewData["Departments"] = _raptor.Departments
                .FromSqlRaw("SELECT * FROM DepartmentMappings")
                .Select(o => o.Department)
                .Distinct().ToList();

            var duplicate = _context.MachineLineAreas?
                .Where(o => o.Name.ToLower() == machineLine.Name.ToLower() && o.Type == machineLine.Type &&
                       o.Department == machineLine.Department && o.OperatingTime == machineLine.OperatingTime)
                .ToList()
                .Count();

            if (id != machineLine.MachineLineAreaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (duplicate == 0)
                {
                    try
                    {
                        _context.Update(machineLine);
                        await _context.SaveChangesAsync();
                        _toasts.Success("Zmiany zostały zapisane", 5);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MachineLineExists(machineLine.MachineLineAreaId))
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
                    _toasts.Custom("Wpis już istnieje", 5, "#f9b30f", "fa fa-warning text-white");
                    return RedirectToAction("Edit", "MachineLines",
                        new { id = machineLine.MachineLineAreaId });
                }
            }
            else
            {
                _toasts.Error("Wystąpił błąd! Zmiany nie zostały zapisane", 5);
                return RedirectToAction("Edit", "MachineLines",
                    new { id = machineLine.MachineLineAreaId });
            }
        }

        // GET: MachineLines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MachineLineAreas == null)
            {
                return NotFound();
            }

            var machineLine = await _context.MachineLineAreas
                .FirstOrDefaultAsync(m => m.MachineLineAreaId == id);
            if (machineLine == null)
            {
                return NotFound();
            }

            return View(machineLine);
        }

        // POST: MachineLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MachineLineAreas == null)
            {
                return Problem("Entity set 'TCZNT5000.MachinesLines'  is null.");
            }
            var machineLine = await _context.MachineLineAreas.FindAsync(id);
            if (machineLine != null)
            {
                _context.MachineLineAreas.Remove(machineLine);
                if (machineLine.Type == 0)
                {
                    _toasts.Success("Obszar został usunięty", 5);
                }
                else if (machineLine.Type == 1)
                {
                    _toasts.Success("Maszyna została usunięta", 5);
                }
                else
                {
                    _toasts.Success("Linia została usunięta", 5);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MachineLineExists(int id)
        {
            return (_context.MachineLineAreas?.Any(e => e.MachineLineAreaId == id)).GetValueOrDefault();
        }
    }
}
