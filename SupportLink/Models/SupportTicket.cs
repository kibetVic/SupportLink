using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportLink.Models
{
    public class SupportTicket
    {
        [Key]
        public int SupportId { get; set; }

        // 🔹 Foreign Keys (Required except AssignedAgentId)
        [Required]
        [Display(Name = "Created By")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Organization")]
        public int OrganizationId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int StatusId { get; set; }

        [Display(Name = "Assigned Agent")]
        public int? AssignedAgentId { get; set; } // optional

        // 🔹 Ticket Details
        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Display(Name = "File Type")]
        public int? FileTypeId { get; set; }   // now a foreign key
        public string? UploadFile { get; set; }

        [NotMapped] // not saved in DB, only used for upload
        public IFormFile? FileUpload { get; set; }

        // 🔹 Dates
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        // 🔹 Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual AccountUser? AccountUser { get; set; }

        [ForeignKey(nameof(OrganizationId))]
        public virtual Organization? Organization { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual IssueCategory? IssueCategory { get; set; }

        [ForeignKey(nameof(StatusId))]
        public virtual TicketStatus? Status { get; set; }

        [ForeignKey(nameof(AssignedAgentId))]
        public virtual AccountUser? AssignedAgent { get; set; }

        [ForeignKey(nameof(FileTypeId))]
        public virtual FileType? FileType { get; set; }  // navigation to new table

        // 🔹 Relationships (one-to-many / one-to-one)
        public ICollection<TicketUpdate> Updates { get; set; } = new List<TicketUpdate>();
        public Feedback? Feedback { get; set; }
    }

    public class TicketStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation: One status → many tickets
        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }

    public class IssueCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        // Navigation: One category → many tickets
        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }

    public class FileType
    {
        [Key]
        public int FileTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation: One file type → many tickets
        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }
}
