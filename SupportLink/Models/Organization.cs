using System.ComponentModel.DataAnnotations;

namespace SupportLink.Models
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; } // SACCO or Company Name
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
       // public ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
    }
}
