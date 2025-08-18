using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportLink.Models
{
    public class SupportTicket
    {
        [Key]
        public int SupportId { get; set; }

        // Ticket Creator
        public int UserId { get; set; }
        public virtual AccountUser AccountUser { get; set; }

        // Organization
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        [Required]
        public string IssueCategory { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? FileType { get; set; }

        [NotMapped]
        public IFormFile? Uploads { get; set; }

        public string? UploadFile { get; set; } // File path

        public string Status { get; set; } = "New"; // New, Assigned, In Progress, Resolved, Closed

        // Assignment
        public int? AssignedAgentId { get; set; }
        public virtual AccountUser? AssignedAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Relationships
        public ICollection<TicketUpdate> Updates { get; set; } = new List<TicketUpdate>();
        public Feedback? Feedback { get; set; }
    }
}
