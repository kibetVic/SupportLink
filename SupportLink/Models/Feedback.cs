using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportLink.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        public int UpdateId { get; set; }
        public virtual TicketUpdate UpdateDetails { get; set; }
        public int Rating { get; set; } 
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
