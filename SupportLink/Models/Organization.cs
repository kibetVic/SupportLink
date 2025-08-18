using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupportLink.Models
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrganizationCode { get; set; } = string.Empty;

        [Required]
        public string OrganizationName { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        // Navigation
        public ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
    }
}
