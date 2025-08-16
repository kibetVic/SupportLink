using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportLink.Models;

namespace SupportLink.Controllers
{
    public class SupportTicketsController : Controller
    {
        private readonly SupportLinkDbContext _context;

        public SupportTicketsController(SupportLinkDbContext context)
        {
            _context = context;
        }

        // GET: SupportTickets
        public async Task<IActionResult> Index()
        {
            var supportLinkDbContext = _context.SupportTickets.Include(s => s.AccountUser).Include(s => s.AssignedAgent).Include(s => s.Organization);
            return View(await supportLinkDbContext.ToListAsync());
        }

        // GET: SupportTickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportTicket = await _context.SupportTickets
                .Include(s => s.AccountUser)
                .Include(s => s.AssignedAgent)
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(m => m.SupportId == id);
            if (supportTicket == null)
            {
                return NotFound();
            }

            return View(supportTicket);
        }

        // GET: SupportTickets/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email");
            ViewData["AssignedAgentId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email");
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "Id");
            return View();
        }

        // POST: SupportTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupportId,UserId,OrganizationId,IssueCategory,Description,FileType,UploadFile,Status,AssignedAgentId,AssignedTo,CreatedAt,ResolvedAt")] SupportTicket supportTicket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supportTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email", supportTicket.UserId);
            ViewData["AssignedAgentId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email", supportTicket.AssignedAgentId);
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "Id", supportTicket.OrganizationId);
            return View(supportTicket);
        }

        // GET: SupportTickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportTicket = await _context.SupportTickets.FindAsync(id);
            if (supportTicket == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email", supportTicket.UserId);
            ViewData["AssignedAgentId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email", supportTicket.AssignedAgentId);
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "Id", supportTicket.OrganizationId);
            return View(supportTicket);
        }

        // POST: SupportTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SupportId,UserId,OrganizationId,IssueCategory,Description,FileType,UploadFile,Status,AssignedAgentId,AssignedTo,CreatedAt,ResolvedAt")] SupportTicket supportTicket)
        {
            if (id != supportTicket.SupportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supportTicket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupportTicketExists(supportTicket.SupportId))
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
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email", supportTicket.UserId);
            ViewData["AssignedAgentId"] = new SelectList(_context.ApplicationUsers, "UserId", "Email", supportTicket.AssignedAgentId);
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "Id", supportTicket.OrganizationId);
            return View(supportTicket);
        }

        // GET: SupportTickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supportTicket = await _context.SupportTickets
                .Include(s => s.AccountUser)
                .Include(s => s.AssignedAgent)
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(m => m.SupportId == id);
            if (supportTicket == null)
            {
                return NotFound();
            }

            return View(supportTicket);
        }

        // POST: SupportTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supportTicket = await _context.SupportTickets.FindAsync(id);
            if (supportTicket != null)
            {
                _context.SupportTickets.Remove(supportTicket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupportTicketExists(int id)
        {
            return _context.SupportTickets.Any(e => e.SupportId == id);
        }
    }
}
