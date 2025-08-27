using Microsoft.EntityFrameworkCore;
using SupportLink.Data;
using SupportLink.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportLink.Services
{
    public class ReportService
    {
        private readonly SupportLinkDbContext _context;

        public ReportService(SupportLinkDbContext context)
        {
            _context = context;
        }

        public async Task<List<Report>> GetReportsAsync(ReportFilter filter)
        {
            var query = _context.SupportTickets
                .Include(t => t.Organization)
                .Include(t => t.AccountUser)
                .Include(t => t.AssignedAgent)
                .Include(t => t.IssueCategory)
                .Include(t => t.FileType)
                .Include(t => t.Feedback)
                .Include(t => t.Updates)
                .AsQueryable();

            // Apply Filters
            if (filter.OrganizationId.HasValue)
                query = query.Where(t => t.OrganizationId == filter.OrganizationId);

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId);

            if (filter.StatusId.HasValue)
                query = query.Where(t => t.StatusId == filter.StatusId);

            if (filter.AgentId.HasValue)
                query = query.Where(t => t.AssignedId == filter.AgentId);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate);

            var tickets = await query.ToListAsync();

            return tickets.Select(t => new Report
            {
                TicketId = t.SupportId,
                Description = t.Description,
                OrganizationId = t.OrganizationId,
                OrganizationName = t.Organization?.OrganizationName ?? "",
                CreatedByUserId = t.UserId,
                CreatedByEmail = t.AccountUser?.Email ?? "",
                AssignedAgentId = t.AssignedId,
                AssignedAgentEmail = t.AssignedAgent?.Email,
                CategoryName = t.IssueCategory?.Name ?? "",
                FileTypeName = t.FileType?.Name,
                StatusName = t.Status?.Name ?? "",
                CreatedAt = t.CreatedAt,
                ResolvedAt = t.ResolvedAt,
                FeedbackRating = t.Feedback?.Rating,
                FeedbackComments = t.Feedback?.Comments,
                LastUpdatedAt = t.Updates.OrderByDescending(u => u.UpdatedAt).FirstOrDefault()?.UpdatedAt,
                LastUpdatedBy = t.Updates.OrderByDescending(u => u.UpdatedAt)
                    .FirstOrDefault()?.UpdatedBy?.Email,
                LastUpdateStatus = t.Updates.OrderByDescending(u => u.UpdatedAt)
                    .FirstOrDefault()?.Status.ToString()
            }).ToList();
        }

        internal IEnumerable<object> GetReports(ReportFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
