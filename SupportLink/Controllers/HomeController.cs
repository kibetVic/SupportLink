using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
using SupportLink.Models;
using System.Diagnostics;

namespace SupportLink.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SupportLinkDbContext _context;

        public HomeController(ILogger<HomeController> logger, SupportLinkDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get user details
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.Role = User.IsInRole("Admin") || User.IsInRole("HelpDesk") ? "Admin/HelpDesk"
             : User.IsInRole("Agent") ? "Agent"
             : User.IsInRole("Staff") ? "Staff"
             : "User";

            // Get current user ID from claims
            var claimVal = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrWhiteSpace(claimVal) || !int.TryParse(claimVal, out var userId))
            {
                return Forbid();
            }

            // Role-based dashboard data
            if (User.IsInRole("Admin") || User.IsInRole("HelpDesk"))
            {
                return await GetAdminDashboard();
            }
            else if (User.IsInRole("Staff"))
            {
                return await GetStaffDashboard(userId);
            }
            else if (User.IsInRole("Agent"))
            {
                return await GetAgentDashboard(userId);
            }

            // Default view for other roles
            return View();
        }

        //private async Task<IActionResult> GetAdminDashboard()
        //{
        //    // Unassigned tickets: No agent assigned yet
        //    var unassignedTickets = await _context.SupportTickets
        //        .Include(t => t.Status)
        //        .Where(t => t.AssignedId == null) // Adjust this to your actual column name!
        //        .ToListAsync();

        //    // 🔹 Fetch available agents (Admin, HelpDesk, Agent)
        //    //ViewBag.Agents = await _context.Users
        //    //      .Where(u => u.Role == UserRole.Agent ||
        //    //                  u.Role == UserRole.HelpDesk ||
        //    //                  u.Role == UserRole.Admin)
        //    //      .ToListAsync();

        //    // 🔹 Restrict assignable users to all roles except Staff
        //    ViewBag.Agents = await _context.Users
        //        .Where(u => u.Role != UserRole.Staff)
        //        .ToListAsync();

        //    //ViewBag.Agents = await _context.Users
        //    //     .Where(u => u.Role == UserRole.Agent) // Adjust according to your roles
        //    //     .ToListAsync();

        //    int unAssignedCount = await _context.SupportTickets
        //           .CountAsync(t => t.AssignedId == null);
        //    //int unAssignedCount = await _context.SupportTickets
        //    //     .CountAsync(t => t.AssignedId == null && t.StatusId == 1);
        //    int newCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 1);
        //    int assignedCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 2);
        //    int inProgressCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 3);
        //    int resolvedCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 4);
        //    int closedCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 5);

        //    int solved = resolvedCount + closedCount;
        //    int unsolved = newCount + assignedCount + inProgressCount;
        //    int total = solved + unsolved;

        //    var vm = new DashboardViewModel
        //    {
        //        UnAssignedCount = unAssignedCount,
        //        NewCount = newCount,
        //        AssignedCount = assignedCount,
        //        InProgressCount = inProgressCount,
        //        ResolvedCount = resolvedCount,
        //        ClosedCount = closedCount,
        //        SolvedCount = solved,
        //        UnsolvedCount = unsolved,
        //        TotalTickets = total,

        //        // Unsolved tickets (New, Assigned, or InProgress)
        //        UnsolvedTickets = await _context.SupportTickets
        //             .Where(t => t.AssignedId == null && (t.StatusId == 1 || t.StatusId == 2 || t.StatusId == 3))
        //             .Include(t => t.Status)
        //             .Include(t => t.AssignedAgent)
        //             .OrderByDescending(t => t.CreatedAt)
        //             .Take(10)
        //             .ToListAsync(),

        //        // Only Unassigned & New tickets
        //        UnAssignedTickets = await _context.SupportTickets
        //            .Where(t => t.AssignedId == null /*&& t.StatusId == 1*/)
        //            .Include(t => t.Status)
        //            .Include(t => t.AssignedAgent)
        //            .OrderByDescending(t => t.CreatedAt)
        //            .Take(10)
        //            .ToListAsync(),

        //        // Solved tickets (Resolved or Closed)
        //        SolvedTickets = await _context.SupportTickets
        //            .Where(t => t.StatusId == 4 || t.StatusId == 5)
        //            .Include(t => t.Status)
        //            .OrderByDescending(t => t.ResolvedAt ?? t.CreatedAt)
        //            .Take(10)
        //            .ToListAsync(),

        //        UnsolvedTickets = await _context.SupportTickets
        //           .Where(t => t.StatusId == 1 || t.StatusId == 2 || t.StatusId == 3)
        //           .Include(t => t.Status)
        //           .Include(t => t.AssignedAgent)
        //           .OrderByDescending(t => t.CreatedAt)
        //           .Take(10)
        //           .ToListAsync();
        //    };

        //    ViewBag.DashboardType = "Admin/HelpDesk";
        //    return View("AdminDashboard", vm);
        //}


        private async Task<IActionResult> GetAdminDashboard()
        {
            // Unassigned tickets: No agent assigned yet
            var unassignedTickets = await _context.SupportTickets
                .Include(t => t.Status)
                .Where(t => t.AssignedId == null) // Adjust this to your actual column name!
                .ToListAsync();

            // 🔹 Restrict assignable users to all roles except Staff
            ViewBag.Agents = await _context.Users
                .Where(u => u.Role != UserRole.Staff)
                .ToListAsync();

            // Ticket counts
            int unAssignedCount = await _context.SupportTickets
                .CountAsync(t => t.AssignedId == null);

            int newCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 1);
            int assignedCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 2);
            int inProgressCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 3);
            int resolvedCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 4);
            int closedCount = await _context.SupportTickets.CountAsync(t => t.StatusId == 5);

            int solved = resolvedCount + closedCount;
            int unsolved = newCount + assignedCount + inProgressCount;
            int total = solved + unsolved;

            var vm = new DashboardViewModel
            {
                UnAssignedCount = unAssignedCount,
                NewCount = newCount,
                AssignedCount = assignedCount,
                InProgressCount = inProgressCount,
                ResolvedCount = resolvedCount,
                ClosedCount = closedCount,
                SolvedCount = solved,
                UnsolvedCount = unsolved,
                TotalTickets = total,

                // 🔹 Unsolved tickets (New, Assigned, or InProgress) - all unsolved tickets
                UnsolvedTickets = await _context.SupportTickets
                    .Where(t => t.StatusId == 1 || t.StatusId == 2 || t.StatusId == 3)
                    .Include(t => t.Status)
                    .Include(t => t.AssignedAgent)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync(),

                // 🔹 Only Unassigned tickets (regardless of status)
                UnAssignedTickets = await _context.SupportTickets
                    .Where(t => t.AssignedId == null)
                    .Include(t => t.Status)
                    .Include(t => t.AssignedAgent)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync(),

                // 🔹 Solved tickets (Resolved or Closed)
                SolvedTickets = await _context.SupportTickets
                    .Where(t => t.StatusId == 4 || t.StatusId == 5)
                    .Include(t => t.Status)
                    .OrderByDescending(t => t.ResolvedAt ?? t.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            ViewBag.DashboardType = "Admin/HelpDesk";
            return View("AdminDashboard", vm);
        }



        private async Task<IActionResult> GetStaffDashboard(int userId)
        {
            int myNew = await _context.SupportTickets.CountAsync(t => t.UserId == userId && t.StatusId == 1);
            int myAssigned = await _context.SupportTickets.CountAsync(t => t.UserId == userId && t.StatusId == 2);
            int myInProgress = await _context.SupportTickets.CountAsync(t => t.UserId == userId && t.StatusId == 3);
            int myResolved = await _context.SupportTickets.CountAsync(t => t.UserId == userId && t.StatusId == 4);
            int myClosed = await _context.SupportTickets.CountAsync(t => t.UserId == userId && t.StatusId == 5);

            int myOpen = myNew + myAssigned + myInProgress;
            int mySolved = myResolved + myClosed;
            int myTotal = myOpen + mySolved;

            int myUpdates = await _context.TicketUpdates.CountAsync(u => u.UpdatedById == userId);
            int myFeedback = await _context.Feedbacks.CountAsync(f => f.UserId == userId);

            var recentTickets = await _context.SupportTickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Status)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new StaffDashboardViewModel
            {
                MyOpenCount = myOpen,
                MySolvedCount = mySolved,
                MyTotalCount = myTotal,
                MyNewCount = myNew,
                MyAssignedCount = myAssigned,
                MyInProgressCount = myInProgress,
                MyResolvedCount = myResolved,
                MyClosedCount = myClosed,
                MyUpdatesCount = myUpdates,
                MyFeedbackCount = myFeedback,
                RecentTickets = recentTickets,
                MySolvedTickets = await _context.SupportTickets
                    .Where(t => t.UserId == userId && (t.StatusId == 4 || t.StatusId == 5))
                    .Include(t => t.Status)
                    .OrderByDescending(t => t.ResolvedAt ?? t.CreatedAt)
                    .Take(10)
                    .ToListAsync(),
                MyUnsolvedTickets = await _context.SupportTickets
                    .Where(t => t.UserId == userId && (t.StatusId == 1 || t.StatusId == 2 || t.StatusId == 3))
                    .Include(t => t.Status)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            ViewBag.DashboardType = "Staff";
            return View("StaffDashboard", vm);
        }

        private async Task<IActionResult> GetAgentDashboard(int userId)
        {
            int assignedToMe = await _context.SupportTickets.CountAsync(t => t.AssignedId == userId && (t.StatusId == 2 || t.StatusId == 3));
            int newlyAssigned = await _context.SupportTickets.CountAsync(t => t.AssignedId == userId && t.StatusId == 2);
            int inProgress = await _context.SupportTickets.CountAsync(t => t.AssignedId == userId && t.StatusId == 3);
            int myResolved = await _context.SupportTickets.CountAsync(t => t.AssignedId == userId && t.StatusId == 4);
            int myClosed = await _context.SupportTickets.CountAsync(t => t.AssignedId == userId && t.StatusId == 5);

            int mySolved = myResolved + myClosed;

            int updatesMade = await _context.TicketUpdates.CountAsync(u => u.UpdatedById == userId);

            var recentAssigned = await _context.SupportTickets
                .Where(t => t.AssignedId == userId)
                .Include(t => t.Status)
                .OrderByDescending(t => t.CreatedAt)
                .Take(7)
                .ToListAsync();

            var vm = new AgentDashboardViewModel
            {
                AssignedOpen = assignedToMe,
                NewlyAssigned = newlyAssigned,
                InProgress = inProgress,
                Solved = mySolved,
                UpdatesMade = updatesMade,
                RecentAssignedTickets = recentAssigned
            };

            ViewBag.DashboardType = "Agent";
            return View("AgentDashboard", vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
