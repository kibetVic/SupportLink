using System;
using System.ComponentModel.DataAnnotations;

namespace SupportLink.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        // Linked Ticket
        public int SupportId { get; set; }
        public virtual SupportTicket? Ticket { get; set; }

        // Who gave feedback
        public int UserId { get; set; }
        public virtual AccountUser? User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
