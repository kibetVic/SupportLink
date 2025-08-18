using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
using SupportLink.Models;

namespace SupportLink.Controllers
{
    public class TicketUpdatesController : Controller
    {
        private readonly SupportLinkDbContext _context;

        public TicketUpdatesController(SupportLinkDbContext context)
        {
            _context = context;
        }

        // GET: TicketUpdates
        public async Task<IActionResult> Index()
        {
            var supportLinkDbContext = _context.TicketUpdates.Include(t => t.Ticket).Include(t => t.UpdatedBy);
            return View(await supportLinkDbContext.ToListAsync());
        }

        // GET: TicketUpdates/Details/5
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

            return View(ticketUpdate);
        }

        // GET: TicketUpdates/Create
        public IActionResult Create()
        {
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "Description");
            ViewData["UpdatedById"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: TicketUpdates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SupportId,UpdatedById,UpdateDetails,UpdatedAt")] TicketUpdate ticketUpdate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "Description", ticketUpdate.SupportId);
            ViewData["UpdatedById"] = new SelectList(_context.Users, "UserId", "Email", ticketUpdate.UpdatedById);
            return View(ticketUpdate);
        }

        // GET: TicketUpdates/Edit/5
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
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "Description", ticketUpdate.SupportId);
            ViewData["UpdatedById"] = new SelectList(_context.Users, "UserId", "Email", ticketUpdate.UpdatedById);
            return View(ticketUpdate);
        }

        // POST: TicketUpdates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SupportId,UpdatedById,UpdateDetails,UpdatedAt")] TicketUpdate ticketUpdate)
        {
            if (id != ticketUpdate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["SupportId"] = new SelectList(_context.SupportTickets, "SupportId", "Description", ticketUpdate.SupportId);
            ViewData["UpdatedById"] = new SelectList(_context.Users, "UserId", "Email", ticketUpdate.UpdatedById);
            return View(ticketUpdate);
        }

        // GET: TicketUpdates/Delete/5
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
