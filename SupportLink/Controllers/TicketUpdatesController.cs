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
    public class TicketUpdatesController : Controller
    {
        private readonly SupportLinkDbContext _context;

        public TicketUpdatesController(SupportLinkDbContext context)
        {
            _context = context;
        }

        // GET: TicketUpdates
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Index()
        {
            var updates = _context.TicketUpdates
                .Include(t => t.Ticket)
                .Include(t => t.UpdatedBy)
                .AsQueryable();

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (User.IsInRole("Admin") || User.IsInRole("HelpDesk"))
                {
                    // Admin and HelpDesk see all
                }
                else if (User.IsInRole("Agent"))
                {
                    updates = updates.Where(u =>
                        u.UpdatedById == currentUserId ||
                        u.Ticket.AssignedId == currentUserId ||
                        u.Ticket.UserId == currentUserId);
                }
                else
                {
                    updates = updates.Where(u =>
                        u.UpdatedById == currentUserId ||
                        u.Ticket.UserId == currentUserId);
                }
            }

            return View(await updates.ToListAsync());
        }

        // GET: TicketUpdates/Details/5
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketUpdate = await _context.TicketUpdates
                .Include(t => t.Ticket)
                .Include(t => t.UpdatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketUpdate == null)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) &&
                int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    if (!(ticketUpdate.UpdatedById == currentUserId ||
                          ticketUpdate.Ticket.UserId == currentUserId))
                    {
                        return Forbid();
                    }
                }
            }

            return View(ticketUpdate);
        }

        // GET: TicketUpdates/Create
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public IActionResult Create(int? supportId)
        {
            if (supportId.HasValue)
            {
                var ticket = _context.SupportTickets.Include(t => t.Status)
                    .FirstOrDefault(t => t.SupportId == supportId.Value);
                if (ticket == null) return NotFound();

                var claimVal = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrWhiteSpace(claimVal) &&
                    int.TryParse(claimVal, out var currentUserId))
                {
                    if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk") ||
                          ticket.AssignedId == currentUserId ||
                          ticket.UserId == currentUserId))
                    {
                        return Forbid();
                    }
                }

                ViewBag.PreselectedSupportId = ticket.SupportId;
                ViewBag.PreselectedTicketDescription = ticket.Description;
            }
            else
            {
                ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName");
            }

            return View();
        }

        // POST: TicketUpdates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Create([Bind("Id,SupportId,UpdatedById,Status,UpdatedAt")] TicketUpdate ticketUpdate)
        {
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) &&
                int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                ticketUpdate.UpdatedById = currentUserId;
            }
            ticketUpdate.UpdatedAt = DateTime.UtcNow;

            ModelState.Remove("UpdatedById");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                _context.Add(ticketUpdate);

                var linked = await _context.SupportTickets
                    .FirstOrDefaultAsync(t => t.SupportId == ticketUpdate.SupportId);
                if (linked != null)
                {
                    if (ticketUpdate.Status == UpdateStatus.Solved)
                    {
                        linked.StatusId = 4;
                        linked.ResolvedAt = DateTime.UtcNow;
                        TempData["ToastMessage"] = "Your ticket has been solved";
                        TempData["ToastType"] = "success";
                    }
                    else if (ticketUpdate.Status == UpdateStatus.NotSolved &&
                             (linked.StatusId == 4 || linked.StatusId == 5))
                    {
                        linked.StatusId = 3;
                        linked.ResolvedAt = null;
                        TempData["ToastMessage"] = "Ticket marked as not solved";
                        TempData["ToastType"] = "warning";
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName",
                ticketUpdate.SupportId);
            return View(ticketUpdate);
        }

        // GET: TicketUpdates/Edit/5
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketUpdate = await _context.TicketUpdates.FindAsync(id);
            if (ticketUpdate == null)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) &&
                int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (User.IsInRole("Staff"))
                {
                    if (ticketUpdate.UpdatedById != currentUserId) return Forbid();
                }
                else if (User.IsInRole("Agent") && 
                         !(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    if (!(ticketUpdate.UpdatedById == currentUserId ||
                          ticketUpdate.Ticket.UserId == currentUserId ||
                          ticketUpdate.Ticket.AssignedId == currentUserId))
                        return Forbid();
                }
            }

            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName",
                ticketUpdate.SupportId);
            ViewData["UpdatedById"] = new SelectList(_context.Users, "UserId", "Email", ticketUpdate.UpdatedById);
            return View(ticketUpdate);
        }

        // POST: TicketUpdates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,SupportId,UpdatedById,Status,UpdatedAt")] TicketUpdate ticketUpdate)
        {
            if (id != ticketUpdate.Id)
            {
                return NotFound();
            }

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) &&
                int.TryParse(currentUserIdClaim, out var currentUserIdForEdit))
            {
                var original = await _context.TicketUpdates.Include(t => t.Ticket).AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);
                if (original == null) return NotFound();

                if (User.IsInRole("Staff"))
                {
                    if (original.UpdatedById != currentUserIdForEdit) return Forbid();
                }
                else if (User.IsInRole("Agent") &&
                         !(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    if (!(original.UpdatedById == currentUserIdForEdit ||
                          (original.Ticket != null &&
                           (original.Ticket.UserId == currentUserIdForEdit ||
                            original.Ticket.AssignedId == currentUserIdForEdit))))
                        return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk") || User.IsInRole("Agent")))
                    {
                        var claimVal = User.FindFirst("UserId")?.Value;
                        if (!string.IsNullOrWhiteSpace(claimVal) && int.TryParse(claimVal, out var uid))
                        {
                            ticketUpdate.UpdatedById = uid;
                        }
                    }
                    _context.Update(ticketUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketUpdateExists(ticketUpdate.Id))
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

            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "TicketName",
                ticketUpdate.SupportId);
            ViewData["UpdatedById"] = new SelectList(_context.Users, "UserId", "Email", ticketUpdate.UpdatedById);
            return View(ticketUpdate);
        }

        // GET: TicketUpdates/Delete/5
        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketUpdate = await _context.TicketUpdates
                .Include(t => t.Ticket)
                .Include(t => t.UpdatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketUpdate == null)
            {
                return NotFound();
            }

            return View(ticketUpdate);
        }

        // POST: TicketUpdates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketUpdate = await _context.TicketUpdates.FindAsync(id);
            if (ticketUpdate != null)
            {
                _context.TicketUpdates.Remove(ticketUpdate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketUpdateExists(int id)
        {
            return _context.TicketUpdates.Any(e => e.Id == id);
        }
    }
}
