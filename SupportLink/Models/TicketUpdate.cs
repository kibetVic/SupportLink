using System;
using System.ComponentModel.DataAnnotations;

namespace SupportLink.Models
{
    public class TicketUpdate
    {
        [Key]
        public int Id { get; set; }

        // Linked Ticket
        public int SupportId { get; set; }
        public virtual SupportTicket Ticket { get; set; }

        // Who updated
        public int UpdatedById { get; set; }
        public virtual AccountUser UpdatedBy { get; set; }

        [Required]
        public string UpdateDetails { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
