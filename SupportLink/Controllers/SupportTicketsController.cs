using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SupportLink.Data;
using SupportLink.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportLink.Models;
using System.IO;
using System.Threading.Tasks;

namespace SupportLink.Controllers
{
    public class SupportTicketsController : Controller
    {
        private readonly SupportLinkDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SupportTicketsController(SupportLinkDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: SupportTickets
        public async Task<IActionResult> Index(string searchString)
        {
            var tickets = _context.SupportTickets
                .Include(t => t.Organization)
                .Include(t => t.IssueCategory)
                .Include(t => t.Status)
                .Include(t => t.AssignedAgent)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Filter tickets to only those that match the search
                tickets = tickets.Where(t =>
                    t.Organization.OrganizationCode.Contains(searchString) ||
                    t.IssueCategory.Name.Contains(searchString) ||
                    t.Status.Name.Contains(searchString) ||
                    (t.AssignedAgent != null && t.AssignedAgent.Email.Contains(searchString))
                );
            }

            ViewData["CurrentFilter"] = searchString;
            return View(await tickets.ToListAsync());
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
                .Include(s => s.IssueCategory)
                .Include(s => s.Organization)
                .Include(s => s.Status)
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
            ViewBag.Users = new SelectList(_context.Users, "UserId", "Email");
            ViewBag.AssignedAgentId = new SelectList(_context.Users, "UserId", "Email");
            ViewBag.Categories = new SelectList(_context.IssueCategories, "CategoryId", "Name");
            ViewBag.Organizations = new SelectList(_context.Organizations, "Id", "OrganizationCode");
            ViewBag.Statuses = new SelectList(_context.TicketStatuses, "StatusId", "Name");
            ViewBag.FileTypes = new SelectList(_context.FileTypes, "FileTypeId", "Name"); // <-- new
            return View();
        }

        // POST: SupportTickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportTicket supportTicket)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // 🔹 Handle File Upload
                    if (supportTicket.FileUpload != null && supportTicket.FileUpload.Length > 0)
                    {
                        string uploadDir = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        string uniqueFileName = Guid.NewGuid() + Path.GetExtension(supportTicket.FileUpload.FileName);
                        string filePath = Path.Combine(uploadDir, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await supportTicket.FileUpload.CopyToAsync(stream);
                        }

                        supportTicket.UploadFile = "/uploads/" + uniqueFileName;

                        // 🔹 Auto-detect FileType
                        string ext = Path.GetExtension(supportTicket.FileUpload.FileName).ToLower();
                        if (ext is ".jpg" or ".jpeg" or ".png" or ".gif")
                            supportTicket.FileTypeId = _context.FileTypes.FirstOrDefault(f => f.Name.ToLower() == "image")?.FileTypeId;
                        else if (ext is ".doc" or ".docx")
                            supportTicket.FileTypeId = _context.FileTypes.FirstOrDefault(f => f.Name.ToLower() == "document")?.FileTypeId;
                        else if (ext == ".pdf")
                            supportTicket.FileTypeId = _context.FileTypes.FirstOrDefault(f => f.Name.ToLower() == "pdf")?.FileTypeId;
                        else
                            supportTicket.FileTypeId = _context.FileTypes.FirstOrDefault(f => f.Name.ToLower() == "others")?.FileTypeId;
                    }

                    // 🔹 Set defaults
                    supportTicket.StatusId = 1; // Default: "New"
                    supportTicket.CreatedAt = DateTime.UtcNow;

                    _context.Add(supportTicket);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Support ticket created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["ErrorMessage"] = "⚠️ Ticket could not be created. Errors: " + string.Join("; ", errors);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ An error occurred while saving the ticket: " + ex.Message;
            }

            // ❌ If invalid → re-populate dropdowns
            ViewBag.Users = new SelectList(_context.Users, "UserId", "Email", supportTicket.UserId);
            ViewBag.AssignedAgentId = new SelectList(_context.Users, "UserId", "Email", supportTicket.AssignedAgentId);
            ViewBag.Categories = new SelectList(_context.IssueCategories, "CategoryId", "Name", supportTicket.CategoryId);
            ViewBag.Organizations = new SelectList(_context.Organizations, "Id", "OrganizationCode", supportTicket.OrganizationId);
            ViewBag.Statuses = new SelectList(_context.TicketStatuses, "StatusId", "Name", supportTicket.StatusId);
            ViewBag.FileTypes = new SelectList(_context.FileTypes, "FileTypeId", "Name", supportTicket.FileTypeId);

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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", supportTicket.UserId);
            ViewData["AssignedAgentId"] = new SelectList(_context.Users, "UserId", "Email", supportTicket.AssignedAgentId);
            ViewData["CategoryId"] = new SelectList(_context.IssueCategories, "CategoryId", "Name", supportTicket.CategoryId);
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "OrganizationCode", supportTicket.OrganizationId);
            ViewData["StatusId"] = new SelectList(_context.TicketStatuses, "StatusId", "Name", supportTicket.StatusId);
            return View(supportTicket);
        }

        // POST: SupportTickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SupportId,UserId,OrganizationId,CategoryId,Description,FileType,UploadFile,StatusId,AssignedAgentId,CreatedAt,ResolvedAt")] SupportTicket supportTicket)
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", supportTicket.UserId);
            ViewData["AssignedAgentId"] = new SelectList(_context.Users, "UserId", "Email", supportTicket.AssignedAgentId);
            ViewData["CategoryId"] = new SelectList(_context.IssueCategories, "CategoryId", "Name", supportTicket.CategoryId);
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "OrganizationCode", supportTicket.OrganizationId);
            ViewData["StatusId"] = new SelectList(_context.TicketStatuses, "StatusId", "Name", supportTicket.StatusId);
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
                .Include(s => s.IssueCategory)
                .Include(s => s.Organization)
                .Include(s => s.Status)
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
