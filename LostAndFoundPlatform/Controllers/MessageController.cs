using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LostAndFoundPlatform.Data;
using LostAndFoundPlatform.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LostAndFoundPlatform.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Message
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Messages.Include(m => m.Receiver).Include(m => m.Report).Include(m => m.Sender);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Message/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Report)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Message/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ReportId"] = new SelectList(_context.Reports, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Message/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,SentAt,SenderId,ReceiverId,ReportId")] Message message)
        {
            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", message.ReceiverId);
            ViewData["ReportId"] = new SelectList(_context.Reports, "Id", "Id", message.ReportId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", message.SenderId);
            return View(message);
        }

        // GET: Message/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", message.ReceiverId);
            ViewData["ReportId"] = new SelectList(_context.Reports, "Id", "Id", message.ReportId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", message.SenderId);
            return View(message);
        }

        // POST: Message/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,SentAt,SenderId,ReceiverId,ReportId")] Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
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
            ViewData["ReceiverId"] = new SelectList(_context.Users, "Id", "Id", message.ReceiverId);
            ViewData["ReportId"] = new SelectList(_context.Reports, "Id", "Id", message.ReportId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", message.SenderId);
            return View(message);
        }

        // GET: Message/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Report)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Message/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
        
        // dodadeni novi funkcionalnosti za chat i send
        
        [Authorize]
        public async Task<IActionResult> Chat(int reportId, string? otherUserId)
        {
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var report = await _context.Reports
                .Include(r => r.ApplicationUser)
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return NotFound();
            }

            // Ако не е owner, другиот корисник автоматски е owner-от
            if (currentUserId != report.ApplicationUserId)
            {
                otherUserId = report.ApplicationUserId;
            }

            // Ако owner го отвора chat-от,
            // мора да знаеме со кој корисник разговара
            if (string.IsNullOrEmpty(otherUserId))
            {
                return BadRequest();
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m =>
                    m.ReportId == reportId &&
                    (
                        (m.SenderId == currentUserId &&
                         m.ReceiverId == otherUserId)
                        ||
                        (m.SenderId == otherUserId &&
                         m.ReceiverId == currentUserId)
                    ))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.Report = report;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserId = otherUserId;

            return View(messages);
        }
        
        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
            int reportId,
            string receiverId,
            string content)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Chat", new
                {
                    reportId,
                    otherUserId = receiverId
                });
            }

            var message = new Message
            {
                ReportId = reportId,
                SenderId = currentUserId!,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new
            {
                reportId,
                otherUserId = receiverId
            });
        }
        
        
        [Authorize]
        public async Task<IActionResult> MyMessages()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Report)
                .ThenInclude(r => r.Item)
                .Where(m =>
                    m.SenderId == currentUserId ||
                    m.ReceiverId == currentUserId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            ViewBag.CurrentUserId = currentUserId;

            return View(messages);
        }
        
        
    }
}
