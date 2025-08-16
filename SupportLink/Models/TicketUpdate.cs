using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportLink.Models
{
    public class TicketUpdate
    {
        [Key]
        public int Id { get; set; }
        public int SupportId { get; set; }
        public virtual SupportTicket Ticket { get; set; }
        public int UpdatedById { get; set; }
        public AccountUser UpdatedBy { get; set; }
        public string UpdateDetails { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
