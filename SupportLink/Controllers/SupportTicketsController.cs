using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
using SupportLink.Models;
using SupportLink.Services;
using System.Security.Claims;

namespace SupportLink.Controllers
{
    [Authorize]
    public class SupportTicketsController : Controller
    {
        private readonly SupportLinkDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;
        private readonly ILogger<SupportTicketsController> _logger;

        public SupportTicketsController(SupportLinkDbContext context,
                                        IWebHostEnvironment environment,
                                        IEmailService emailService,
                                        ILogger<SupportTicketsController> logger)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: SupportTickets
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Index(string searchString)
        {
            var tickets = _context.SupportTickets
                .Include(t => t.Organization)
                .Include(t => t.IssueCategory)
                .Include(t => t.Status)
                .Include(t => t.AssignedAgent)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            // Scope by role
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (User.IsInRole("Admin") || User.IsInRole("HelpDesk"))
                {
                    // Admin and HelpDesk see all tickets
                }
                else if (User.IsInRole("Agent"))
                {
                    // Agent sees tickets they created or are assigned to
                    tickets = tickets.Where(t => t.UserId == currentUserId || t.AssignedId == currentUserId);
                }
                else
                {
                    // Staff see only their own tickets
                    tickets = tickets.Where(t => t.UserId == currentUserId);
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                tickets = tickets.Where(t =>
                    t.Organization.OrganizationName.Contains(searchString) ||
                    t.IssueCategory.Name.Contains(searchString) ||
                    t.Status.Name.Contains(searchString) ||
                    (t.AssignedAgent != null && t.AssignedAgent.Email.Contains(searchString))
                );
            }

            ViewBag.Agents = new SelectList(_context.Users.Where(u => u.Role == UserRole.Agent).OrderBy(u => u.Email), "UserId", "Email");
            return View(await tickets.ToListAsync());
        }

        // GET: SupportTickets/Details/5
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var supportTicket = await _context.SupportTickets
                .Include(s => s.AccountUser)
                .Include(s => s.AssignedAgent)
                .Include(s => s.IssueCategory)
                .Include(s => s.Organization)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.SupportId == id);
            if (supportTicket == null)
                return NotFound();

            // Enforce access
            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk") || User.IsInRole("Agent")))
                {
                    if (supportTicket.UserId != currentUserId)
                        return Forbid();
                }
                else
                {
                    if (!(supportTicket.UserId == currentUserId || supportTicket.AssignedId == currentUserId))
                    {
                        if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                            return Forbid();
                    }
                }
            }

            return View(supportTicket);
        }

        // GET: SupportTickets/Create
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Create()
        {
            var userId = Convert.ToInt32(User.FindFirst("UserId")?.Value);

            // Categories, Statuses, FileTypes
            ViewBag.Categories = new SelectList(await _context.IssueCategories.ToListAsync(), "CategoryId", "Name");
            ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "StatusId", "Name");
            ViewBag.FileTypes = new SelectList(await _context.FileTypes.ToListAsync(), "FileTypeId", "Name");

            // Assigned Agents: all roles except Staff
            ViewBag.AssignedAgentId = new SelectList(
                await _context.Users.Where(u => u.Role != UserRole.Staff).ToListAsync(),
                "UserId", "Email"
            );

            // Organizations
            if (User.IsInRole("Staff"))
            {
                // Staff: fixed organization
                var staffOrg = await _context.Users
                                     .Where(u => u.UserId == userId)
                                     .Select(u => u.Organization)
                                     .FirstOrDefaultAsync();

                ViewBag.StaffOrganizationId = staffOrg?.Id;
                ViewBag.StaffOrganizationName = staffOrg?.OrganizationName;
            }
            else
            {
                // Other roles: can select organization
                ViewBag.Organizations = new SelectList(await _context.Organizations.ToListAsync(), "Id", "OrganizationName");
            }

            return View();
        }


        // POST: SupportTickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Create(SupportTicket supportTicket)
        {
            try
            {
                // Set logged-in user
                var currentUserIdClaim = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    supportTicket.UserId = currentUserId;
                }

                // Default status if not set
                if (supportTicket.StatusId == 0)
                {
                    if (User.IsInRole("Admin") || User.IsInRole("HelpDesk") || User.IsInRole("Agent"))
                    {
                        supportTicket.StatusId = 6; // For Admin/HelpDesk/Agent (UnAssigned)
                    }
                    else if (User.IsInRole("Staff"))
                    {
                        supportTicket.StatusId = 3; // For Staff dashboard
                    }
                }

                // Restrict assignment only for Staff
                if (User.IsInRole("Staff"))
                {
                    supportTicket.AssignedId = null;
                }

                // Default status if not set
                //if (supportTicket.StatusId == 0)
                //    supportTicket.StatusId = 3; // Default: In Progress

                supportTicket.CreatedAt = DateTime.UtcNow;

                // Restrict agent assignment
                if (!(User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    supportTicket.AssignedId = null; // Regular users cannot assign agents
                }

                ModelState.Remove("UserId");
                ModelState.Remove("StatusId");
                ModelState.Remove("CreatedAt");

                if (ModelState.IsValid)
                {
                    // Handle file upload
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

                        // Detect file type
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

                    _context.Add(supportTicket);
                    await _context.SaveChangesAsync();

                    // 1️⃣ Send email to assigned agent (if any)
                    if (supportTicket.AssignedId.HasValue)
                    {
                        var agent = await _context.Users.FindAsync(supportTicket.AssignedId.Value);
                        if (agent != null)
                        {
                            try
                            {
                                await _emailService.SendEmailAsync(
                                    agent.Email,
                                    "New Support Ticket Assigned",
                                    $"A new support ticket (#{supportTicket.SupportId}) has been assigned to you."
                                );
                            }
                            catch
                            {
                                TempData["ErrorMessage"] = "Ticket saved but failed to send email to assigned agent.";
                            }
                        }
                    }

                    // 2️⃣ Send email notification to HelpDesk
                    try
                    {
                        // Get all users with the HelpDesk role
                        var helpdeskUsers = _context.Users.Where(u => u.Role == UserRole.HelpDesk).ToList();

                        foreach (var helpdesk in helpdeskUsers)
                        {
                            await _emailService.SendEmailAsync(
                                helpdesk.Email,
                                $"New Support Ticket Created -<br>Subject: {supportTicket.TicketName}",                                
                                $"A new support ticket has been created and sent .\r\n " +
                                $" <strong><br>Subject:</strong> {supportTicket.TicketName}<br>\r\n" +
                                $" <strong>Description:</strong> {supportTicket.Description}<br>\r\n  " +
                                $" <strong>Created At:</strong> {supportTicket.CreatedAt:yyyy-MM-dd HH:mm}<br>\r\n  " +
                                $"Please <a href=\"https://supportlink.amtech.com/Account/Login\">log in to the system</a> and vieew the ticket details."
                            );
                        }

                        TempData["SuccessMessage"] = "Ticket created and HelpDesk notified successfully.";
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"Ticket created but failed to notify HelpDesk. Error: {ex.Message}";
                    }


                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Could not create ticket. Errors: " +
                        string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ An error occurred: " + ex.Message;
            }

            // Reload dropdowns
            ViewBag.Categories = new SelectList(_context.IssueCategories, "CategoryId", "Name", supportTicket.CategoryId);
            ViewBag.Organizations = new SelectList(_context.Organizations, "Id", "OrganizationName", supportTicket.OrganizationId);
            ViewBag.Statuses = new SelectList(_context.TicketStatuses, "StatusId", "Name", supportTicket.StatusId);
            ViewBag.FileTypes = new SelectList(_context.FileTypes, "FileTypeId", "Name", supportTicket.FileTypeId);

            if (User.IsInRole("Admin") || User.IsInRole("HelpDesk"))
            {
                ViewBag.AssignedAgentId = new SelectList(_context.Users.Where(u => u.Role == UserRole.Agent), "UserId", "Email", supportTicket.AssignedId);
            }

            return View(supportTicket);
        }

        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var supportTicket = await _context.SupportTickets.FindAsync(id);
            if (supportTicket == null)
                return NotFound();

            var currentUserIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserIdClaim) && int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                // Staff can only edit their own tickets
                if (User.IsInRole("Staff"))
                {
                    if (supportTicket.UserId != currentUserId)
                        return Forbid();
                }
                // Agents can edit their own tickets or assigned ones
                else if (User.IsInRole("Agent") && (User.IsInRole("Admin") || User.IsInRole("HelpDesk")))
                {
                    if (!(supportTicket.UserId == currentUserId || supportTicket.AssignedId == currentUserId))
                        return Forbid();
                }
            }

            // Allow all roles except Staff to be assigned
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", supportTicket.UserId);
            ViewData["AssignedId"] = new SelectList(_context.Users.Where(u => u.Role != UserRole.Staff), "UserId", "Email", supportTicket.AssignedId);
            ViewData["CategoryId"] = new SelectList(_context.IssueCategories, "CategoryId", "Name", supportTicket.CategoryId);
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "Id", "OrganizationName", supportTicket.OrganizationId);
            ViewData["StatusId"] = new SelectList(_context.TicketStatuses, "StatusId", "Name", supportTicket.StatusId);
            return View(supportTicket);
        }

        // POST: SupportTickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff,Agent,Admin,HelpDesk")]
        public async Task<IActionResult> Edit(int id, SupportTicket supportTicket, IFormFile? Attachment)
        {
            if (id != supportTicket.SupportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTicket = await _context.SupportTickets.AsNoTracking().FirstOrDefaultAsync(t => t.SupportId == id);
                    if (existingTicket == null)
                    {
                        return NotFound();
                    }

                    // Handle attachment update
                    if (Attachment != null && Attachment.Length > 0)
                    {
                        string uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string fileName = Path.GetFileName(Attachment.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Attachment.CopyToAsync(stream);
                        }

                        supportTicket.UploadFile = "/uploads/" + fileName;
                    }
                    else
                    {
                        // Keep the old attachment if no new file is uploaded
                        supportTicket.UploadFile = existingTicket.UploadFile;
                    }

                    _context.Update(supportTicket);
                    await _context.SaveChangesAsync();


                    // Send notification email if the agent changed
                    if(existingTicket.AssignedId != supportTicket.AssignedId)
                    {
                        var newRole = await _context.Users
                            .FirstOrDefaultAsync(u => u.UserId == supportTicket.AssignedId);

                        if (newRole != null && !string.IsNullOrEmpty(newRole.Email) &&
                            (newRole.Role == UserRole.Admin ||
                             newRole.Role == UserRole.HelpDesk ||
                             newRole.Role == UserRole.WBD ||
                             newRole.Role == UserRole.Agent)) // Keep Agent too if needed
                        {
                            try
                            {

                                await _emailService.SendEmailAsync(
                                newRole.Email,
                                "Updated Ticket Assigned",
                                $"A support ticket has been assigned to you.\r\n " +
                                $" <strong><br>Subject:</strong> {supportTicket.TicketName}<br>\r\n" +
                                $" <strong>Description:</strong> {supportTicket.Description}<br>\r\n  " +
                                $" <strong>Created At:</strong> {supportTicket.CreatedAt:yyyy-MM-dd HH:mm}<br>\r\n  " +
                                $"Please <a href=\"https://supportlink.amtech.com/Account/Login\">log in to the system</a> and vieew the ticket details."
                            );

                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send update email to agent {AgentEmail}", newRole.Email);
                                TempData["Warning"] = "Ticket updated, but failed to send notification email to the new agent.";
                            }

                        }
                    }

                    TempData["Success"] = "Ticket updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!SupportTicketExists(supportTicket.SupportId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error while updating ticket {TicketId}", id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating support ticket {TicketId}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the ticket.");
                }
            }

            ViewBag.Users = new SelectList(_context.Users, "UserId", "Email", supportTicket.UserId);
            ViewBag.AssignedId = new SelectList(_context.Users, "UserId", "Email", supportTicket.AssignedId);
            ViewBag.Categories = new SelectList(_context.IssueCategories, "CategoryId", "CategoryName", supportTicket.CategoryId);

            return View(supportTicket);
        }

        private bool SupportTicketExists(int id)
        {
            return _context.SupportTickets.Any(e => e.SupportId == id);
        }


        [HttpPost]
        public async Task<IActionResult> TestSend(string to)
        {
            try
            {
                await _emailService.SendEmailAsync(to, "Mailtrap Live test", "<p>This is a test from SupportLink using Mailtrap Live SMTP.</p>");
                return Content("Sent");
            }
            catch (Exception ex)
            {
                return Content("Failed: " + ex.Message);
            }
        }


        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var supportTicket = await _context.SupportTickets
                .Include(s => s.AccountUser)
                .Include(s => s.AssignedAgent)
                .Include(s => s.IssueCategory)
                .Include(s => s.Organization)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.SupportId == id);
            if (supportTicket == null)
                return NotFound();

            return View(supportTicket);
        }

        // POST: SupportTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HelpDesk")]
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

        // POST: SupportTickets/AssignToMe
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent,Admin,HelpDesk")]
        public async Task<IActionResult> AssignToMe(int id)
        {
            var claimVal = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrWhiteSpace(claimVal) || !int.TryParse(claimVal, out var currentUserId))
            {
                return Forbid();
            }

            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.SupportId == id);
            if (ticket == null)
                return NotFound();

            if (ticket.StatusId == 4 || ticket.StatusId == 5)
            {
                TempData["ToastMessage"] = "Cannot assign a resolved or closed ticket.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            ticket.AssignedId = currentUserId;
            if (ticket.StatusId == 1)
            {
                ticket.StatusId = 2; // Assigned
            }
            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Ticket assigned to you.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // POST: SupportTickets/AssignAgent (Admin/HelpDesk only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> AssignAgent(int id, int agentId)
        {
            var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.SupportId == id);
            if (ticket == null)
                return NotFound();

            if (ticket.StatusId == 4 || ticket.StatusId == 5)
            {
                TempData["ToastMessage"] = "Cannot assign a resolved or closed ticket.";
                TempData["ToastType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var agent = await _context.Users.FirstOrDefaultAsync(u => u.UserId == agentId && u.Role == UserRole.Agent);
            if (agent == null)
            {
                TempData["ToastMessage"] = "Selected user is not a valid agent.";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            ticket.AssignedId = agentId;
            if (ticket.StatusId == 1)
            {
                ticket.StatusId = 2; // Assigned
            }

            await _context.SaveChangesAsync();
            TempData["ToastMessage"] = "Ticket assigned successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,HelpDesk")]
        public async Task<IActionResult> AssignTicket(int ticketId, int agentId)
        {
            var ticket = await _context.SupportTickets
                .Include(t => t.AssignedAgent)
                .FirstOrDefaultAsync(t => t.SupportId == ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            var oldAssignedId = ticket.AssignedId;
            ticket.AssignedId = agentId;

            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();

                // If the agent has changed, send email
                if (oldAssignedId != agentId)
                {
                    var newAgent = await _context.Users.FirstOrDefaultAsync(u => u.UserId == agentId);
                    if (newAgent != null && !string.IsNullOrEmpty(newAgent.Email) &&
                        (newAgent.Role == UserRole.Admin ||
                         newAgent.Role == UserRole.HelpDesk ||
                         newAgent.Role == UserRole.Agent))
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(
                                newAgent.Email,
                                "New Ticket Assigned",
                                $"A support ticket has been assigned to you.<br>" +
                                $"<strong>Subject:</strong> {ticket.TicketName}<br>" +
                                $"<strong>Description:</strong> {ticket.Description}<br>" +
                                $"<strong>Created At:</strong> {ticket.CreatedAt:yyyy-MM-dd HH:mm}<br>" +
                                $"Please <a href=\"https://supportlink.amtech.com/Account/Login\">log in</a> to view it."
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send assignment email to {AgentEmail}", newAgent.Email);
                            return Json(new { success = true, message = "Assigned but failed to send email." });
                        }
                    }
                }

                return Json(new { success = true, message = "Ticket assigned successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign ticket {TicketId}", ticketId);
                return Json(new { success = false, message = "An error occurred while assigning ticket." });
            }
        }


    }
}
