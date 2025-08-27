using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportLink.Models
{
    public class AccountUser
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [NotMapped]   // 🚀 EF won’t try to save this
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Staff; 
        public int? SpecializationCategoryId { get; set; }
        public virtual IssueCategory? SpecializationCategory { get; set; }

        [Required] 
        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }
        // Navigation
        public ICollection<SupportTicket> CreatedTickets { get; set; } = new List<SupportTicket>();
        public ICollection<SupportTicket> AssignedTickets { get; set; } = new List<SupportTicket>();
        public ICollection<TicketUpdate> Updates { get; set; } = new List<TicketUpdate>();
    }

    public enum UserRole
    {
        Admin,
        Agent,
        Staff,
        WBD,
        HelpDesk
    }
}
