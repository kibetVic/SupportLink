using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
using SupportLink.Models;

namespace SupportLink.Controllers
{
    [Authorize]
    public class FeedbacksController : Controller
    {
        private readonly SupportLinkDbContext _context;

        public FeedbacksController(SupportLinkDbContext context)
        {
            _context = context;
        }

        // GET: Feedbacks
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Index()
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.Ticket)
                .Include(f => f.User)
                .AsQueryable();

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                // Admin and HelpDesk see ALL feedbacks (no filtering)
                if (User.IsInRole("Admin") || User.IsInRole("HelpDesk"))
                {
                    // Do not filter
                }
                else if (User.IsInRole("Agent"))
                {
                    var myTicketIds = _context.SupportTickets
                        .Where(t => t.UserId == currentUserId || t.AssignedId == currentUserId)
                        .Select(t => t.SupportId);
                    feedbacks = feedbacks.Where(f => f.UserId == currentUserId || myTicketIds.Contains(f.SupportId));
                }
                else
                {
                    var myTicketIds = _context.SupportTickets
                        .Where(t => t.UserId == currentUserId)
                        .Select(t => t.SupportId);
                    feedbacks = feedbacks.Where(f => f.UserId == currentUserId || myTicketIds.Contains(f.SupportId));
                }
            }

            return View(await feedbacks.ToListAsync());
        }


        // GET: Feedbacks/Details/5
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.Ticket)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk") || User.IsInRole("Agent")))
                {
                    if (!(feedback.UserId == currentUserId || (feedback.Ticket != null && feedback.Ticket.UserId == currentUserId)))
                    {
                        return Forbid();
                    }
                }
            }

            return View(feedback);
        }

        // GET: Feedbacks/Create
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public IActionResult Create(int? supportId)
        {
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName", supportId);
            if (supportId.HasValue)
            {
                ViewBag.LockSupport = true;
                return View(new Feedback { SupportId = supportId.Value });
            }
            return View();
        }

        // POST: Feedbacks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Create([Bind("Id,SupportId,UserId,Rating,Comments,CreatedAt")] Feedback feedback)
        {
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                feedback.UserId = currentUserId;
            }
            feedback.CreatedAt = DateTime.UtcNow;

            ModelState.Remove("UserId");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                var existingFeedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.SupportId == feedback.SupportId);

                if (existingFeedback != null)
                {
                    return RedirectToAction("Edit", new { id = existingFeedback.Id });
                }

                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName", feedback.SupportId);
            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (User.IsInRole("Staff"))
                {
                    if (feedback.UserId != currentUserId) return Forbid();
                }
                else if (User.IsInRole("Agent") && !(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    var ticket = await _context.SupportTickets.AsNoTracking().FirstOrDefaultAsync(t => t.SupportId == feedback.SupportId);
                    if (!(feedback.UserId == currentUserId || (ticket != null && (ticket.UserId == currentUserId || ticket.AssignedId == currentUserId)))) return Forbid();
                }
            }

            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName", feedback.SupportId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", feedback.UserId);
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SupportId,UserId,Rating,Comments,CreatedAt")] Feedback feedback)
        {
            if (id != feedback.Id)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserIdForEdit))
            {
                var original = await _context.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
                if (original == null) return NotFound();
                if (User.IsInRole("Staff"))
                {
                    if (original.UserId != currentUserIdForEdit) return Forbid();
                }
                else if (User.IsInRole("Agent") && !(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    var ticket = await _context.SupportTickets.AsNoTracking().FirstOrDefaultAsync(t => t.SupportId == original.SupportId);
                    if (!(original.UserId == currentUserIdForEdit || (ticket != null && (ticket.UserId == currentUserIdForEdit || ticket.AssignedId == currentUserIdForEdit)))) return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk") || User.IsInRole("Agent")))
                    {
                        var original = await _context.Feedbacks.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
                        if (original != null)
                        {
                            feedback.UserId = original.UserId;
                        }
                    }
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.Id))
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
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName", feedback.SupportId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", feedback.UserId);
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .Include(f => f.Ticket)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
}
