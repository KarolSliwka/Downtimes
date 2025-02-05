using DowntimeTracker.Data;
using DowntimeTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DowntimeTracker.Controllers
{
    [Authorize(Policy = "SuperOnly")]
    public class InfoRecordsController : Controller
    {
        private readonly TCZNT5000 _context;

        public InfoRecordsController(TCZNT5000 context)
        {
            _context = context;
        }

        // GET: InfoRecords
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var records = _context.InfoRecords?
                .OrderBy(o => o.Category)
                .ThenBy(o => o.CorporateReason);
            int pageSize = 20;
            return View(await PaginatedList<InfoRecord>.CreateAsync(records.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: InfoRecords/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: InfoRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InfoRecordId,Category,CorporateReason,M4Category,ToBeClaimed,Type,Example,Explanation,IsVisible,Responsible")] InfoRecord infoRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(infoRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(infoRecord);
        }

        // GET: InfoRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.InfoRecords == null)
            {
                return NotFound();
            }

            var infoRecord = await _context.InfoRecords.FindAsync(id);
            if (infoRecord == null)
            {
                return NotFound();
            }
            return View(infoRecord);
        }

        // POST: InfoRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InfoRecordId,Category,CorporateReason,M4Category,ToBeClaimed,Type,Example,Explanation,IsVisible,Responsible")] InfoRecord infoRecord)
        {
            if (id != infoRecord.InfoRecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(infoRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InfoRecordExists(infoRecord.InfoRecordId))
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
            return View(infoRecord);
        }

        // GET: InfoRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.InfoRecords == null)
            {
                return NotFound();
            }

            var infoRecord = await _context.InfoRecords
                .FirstOrDefaultAsync(m => m.InfoRecordId == id);
            if (infoRecord == null)
            {
                return NotFound();
            }

            return View(infoRecord);
        }

        // POST: InfoRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.InfoRecords == null)
            {
                return Problem("Entity set 'TCZNT5000.InfoRecord'  is null.");
            }
            var infoRecord = await _context.InfoRecords.FindAsync(id);
            if (infoRecord != null)
            {
                _context.InfoRecords.Remove(infoRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InfoRecordExists(int id)
        {
            return (_context.InfoRecords?.Any(e => e.InfoRecordId == id)).GetValueOrDefault();
        }
    }
}
