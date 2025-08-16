using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace SupportLink.Models
{
    public class SupportTicket
    {
        [Key]
        public int SupportId { get; set; }
        public int UserId { get; set; }
        public virtual AccountUser AccountUser { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public string IssueCategory { get; set; } // Payment, Login, Technical, etc.
        public string Description { get; set; }
        public string FileType { get; set; }
        [NotMapped]
        public IFormFile? Uploads { get; set; }
        public string? UploadFile { get; set; } // For path saving to DB
        public string Status { get; set; } = "New"; // New, Assigned, In Progress, Resolved, Closed
        public int AssignedAgentId { get; set; }
        public virtual AccountUser? AssignedAgent { get; set; }
        public string AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        //public ICollection<TicketUpdate> Updates { get; set; } = new List<TicketUpdate>();
        public Feedback? Feedback { get; set; }
    }
    public enum Status
    {
        New,
        Assigned,
        InProgress,
        Resolved,
        Closed
    }
    public enum IssueCategory
    {
        Billing,
        Technicall,
        Payments,
        others,
    }
    public enum FileType
    {
        Sceenshort,
        PDF,
        docx
    }
}
