using System;

namespace SupportLink.Models
{
    public class Report
    {
        public int TicketId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string TicketName { get; set; }

        // Organization
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;

        // Creator
        public int CreatedByUserId { get; set; }
        public string CreatedByEmail { get; set; } = string.Empty;

        // Assigned Agent
        public int? AssignedAgentId { get; set; }
        public string? AssignedAgentEmail { get; set; }

        // Category & File Type
        public string CategoryName { get; set; } = string.Empty;
        public string? FileTypeName { get; set; }

        // Status
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Feedback
        public int? FeedbackRating { get; set; }
        public string? FeedbackComments { get; set; }

        // Latest Update
        public DateTime? LastUpdatedAt { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? LastUpdateStatus { get; set; }
    }

    public class ReportFilter
    {
        public int? OrganizationId { get; set; }
        public int? CategoryId { get; set; }
        public int? StatusId { get; set; }
        public int? AgentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
