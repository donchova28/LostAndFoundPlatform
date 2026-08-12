using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LostAndFoundPlatform.Data;
using LostAndFoundPlatform.Models;
using Microsoft.AspNetCore.Identity;

namespace LostAndFoundPlatform.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        // GET: Report
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reports.Include(r => r.ApplicationUser).Include(r => r.EventLocation).Include(r => r.Item).Include(r => r.PickupLocation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Report/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.ApplicationUser)
                .Include(r => r.EventLocation)
                .Include(r => r.Item)
                .Include(r => r.PickupLocation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Report/Create
        public IActionResult Create()
        {
            ViewData["EventLocationId"] = new SelectList(_context.Locations, "Id", "Address");
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name");
            ViewData["PickupLocationId"] = new SelectList(_context.Locations, "Id", "Address");
            return View();
        }

        // POST: Report/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,CreatedAt,ItemId,EventLocationId,PickupLocationId")] Report report)
        {
            report.ApplicationUserId = _userManager.GetUserId(User);
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventLocationId"] = new SelectList(_context.Locations, "Id", "Address", report.EventLocationId);
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", report.ItemId);
            ViewData["PickupLocationId"] = new SelectList(_context.Locations, "Id", "Address", report.PickupLocationId);
            return View(report);
        }

        // GET: Report/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            ViewData["EventLocationId"] = new SelectList(_context.Locations, "Id", "Address", report.EventLocationId);
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", report.ItemId);
            ViewData["PickupLocationId"] = new SelectList(_context.Locations, "Id", "Address", report.PickupLocationId);
            return View(report);
        }

        // POST: Report/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,CreatedAt,ItemId,EventLocationId,PickupLocationId")] Report report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }
            
            // dodadeno nad proveruvanje modelstate valid !!!!!!!!
            
            var existingReport = await _context.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existingReport == null)
            {
                return NotFound();
            }
            report.ApplicationUserId = existingReport.ApplicationUserId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.Id))
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
            ViewData["EventLocationId"] = new SelectList(_context.Locations, "Id", "Address", report.EventLocationId);
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", report.ItemId);
            ViewData["PickupLocationId"] = new SelectList(_context.Locations, "Id", "Address", report.PickupLocationId);
            return View(report);
        }

        // GET: Report/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.ApplicationUser)
                .Include(r => r.EventLocation)
                .Include(r => r.Item)
                .Include(r => r.PickupLocation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Report/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
