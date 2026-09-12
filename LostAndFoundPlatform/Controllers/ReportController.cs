using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LostAndFoundPlatform.Data;
using LostAndFoundPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LostAndFoundPlatform.Controllers
{
    [Authorize]
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
        public IActionResult Create(int? itemId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var availableItems = _context.Items
                .Where(i =>
                    i.Report == null &&
                    i.ApplicationUserId == currentUserId)
                .ToList();

            if (itemId.HasValue)
            {
                // Прво го бараме Item-от без филтри
                var selectedItem = _context.Items
                    .Include(i => i.Report)
                    .FirstOrDefault(i => i.Id == itemId.Value);

                if (selectedItem == null)
                {
                    return NotFound();
                }

                // Не може да креираш Report за туѓ Item
                if (selectedItem.ApplicationUserId != currentUserId)
                {
                    return Forbid();
                }

                // Ако веќе има Report, оди директно на него
                if (selectedItem.Report != null)
                {
                    return RedirectToAction(
                        "Details",
                        "Report",
                        new { id = selectedItem.Report.Id });
                }

                ViewBag.LockItem = true;
                ViewBag.LockedItemId = selectedItem.Id;
                ViewBag.LockedItemName = selectedItem.Name;
            }
            else
            {
                ViewBag.LockItem = false;
            }

            ViewData["EventLocationId"] =
                new SelectList(_context.Locations, "Id", "Address");

            ViewData["ItemId"] =
                new SelectList(
                    availableItems,
                    "Id",
                    "Name",
                    itemId);

            ViewData["PickupLocationId"] =
                new SelectList(_context.Locations, "Id", "Address");

            return View();
        }

        // POST: Report/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Type,CreatedAt,ItemId,EventLocationId,PickupLocationId")]
            Report report,
            bool lockItem = false)
        {
            var currentUserId = _userManager.GetUserId(User);

            report.ApplicationUserId = currentUserId;

            ModelState.Remove("ApplicationUserId");
            
            report.CreatedAt = DateTime.Now;
            ModelState.Remove("CreatedAt");

            // Проверка дека Item навистина му припаѓа
            // на најавениот корисник
            var validItem = await _context.Items
                .AnyAsync(i =>
                    i.Id == report.ItemId &&
                    i.ApplicationUserId == currentUserId &&
                    i.Report == null);

            if (!validItem)
            {
                ModelState.AddModelError(
                    "ItemId",
                    "The selected item is not available.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(report);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var availableItems = _context.Items
                .Where(i =>
                    i.Report == null &&
                    i.ApplicationUserId == currentUserId)
                .ToList();

            ViewData["EventLocationId"] =
                new SelectList(
                    _context.Locations,
                    "Id",
                    "Address",
                    report.EventLocationId);

            ViewData["ItemId"] =
                new SelectList(
                    availableItems,
                    "Id",
                    "Name",
                    report.ItemId);

            ViewData["PickupLocationId"] =
                new SelectList(
                    _context.Locations,
                    "Id",
                    "Address",
                    report.PickupLocationId);

            ViewBag.LockItem = lockItem;

            if (lockItem)
            {
                var selectedItem = availableItems
                    .FirstOrDefault(i => i.Id == report.ItemId);

                if (selectedItem != null)
                {
                    ViewBag.LockedItemId = selectedItem.Id;
                    ViewBag.LockedItemName = selectedItem.Name;
                }
            }

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

            var currentUserId = _userManager.GetUserId(User);

            // Само сопственикот може да го менува Report-от
            if (report.ApplicationUserId != currentUserId)
            {
                return Forbid();
            }

            // Само Items на моменталниот корисник
            // + тековниот Item мора да остане достапен
            var availableItems = _context.Items
                .Where(i =>
                    i.ApplicationUserId == currentUserId &&
                    (i.Report == null || i.Id == report.ItemId))
                .ToList();

            ViewData["EventLocationId"] =
                new SelectList(_context.Locations, "Id", "Address", report.EventLocationId);

            ViewData["ItemId"] =
                new SelectList(availableItems, "Id", "Name", report.ItemId);

            ViewData["PickupLocationId"] =
                new SelectList(_context.Locations, "Id", "Address", report.PickupLocationId);

            return View(report);
        }

        // POST: Report/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Type,CreatedAt,ItemId,EventLocationId,PickupLocationId")] Report report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            var existingReport = await _context.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReport == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            // ОВА МОРА ДА БИДЕ ПРЕД SaveChanges
            if (existingReport.ApplicationUserId != currentUserId)
            {
                return Forbid();
            }

            // Сопственикот останува ист
            report.ApplicationUserId = existingReport.ApplicationUserId;
            ModelState.Remove("ApplicationUserId");
            
            report.CreatedAt = existingReport.CreatedAt;
            ModelState.Remove("CreatedAt");

            // Дозволени се само 0 = Lost и 1 = Found
            if (report.Type != 0 && report.Type != 1)
            {
                ModelState.AddModelError("Type", "Please select Lost or Found.");
            }

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

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            var availableItems = _context.Items
                .Where(i =>
                    i.ApplicationUserId == currentUserId &&
                    (i.Report == null || i.Id == report.ItemId))
                .ToList();

            ViewData["EventLocationId"] =
                new SelectList(_context.Locations, "Id", "Address", report.EventLocationId);

            ViewData["ItemId"] =
                new SelectList(availableItems, "Id", "Name", report.ItemId);

            ViewData["PickupLocationId"] =
                new SelectList(_context.Locations, "Id", "Address", report.PickupLocationId);

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
            
            // dodadeno za da ne moze sekoj da mene secij report
            if (report.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            

            return View(report);
        }

        // POST: Report/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            if (report.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
