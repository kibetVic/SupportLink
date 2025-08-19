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
        public virtual SupportTicket? Ticket { get; set; }

        // Who updated
        public int UpdatedById { get; set; }
        public virtual AccountUser? UpdatedBy { get; set; }

        [Required]
        public UpdateStatus Status { get; set; }   // use enum instead of string

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum UpdateStatus
    {
        Solved = 1,
        NotSolved = 2
    }
}

