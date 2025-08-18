using System.ComponentModel.DataAnnotations;

namespace SupportLink.Models
{
    public class TicketAttachment
    {
        [Key]
        public int Id { get; set; }

        public int? TicketId { get; set; }
        public SupportTicket? Ticket { get; set; }

        public int? TicketUpdateId { get; set; }
        public TicketUpdate? TicketUpdate { get; set; }

        [Required, StringLength(260)]
        public string StoragePath { get; set; } // e.g., /uploads/...

        [Required, StringLength(180)]
        public string OriginalFileName { get; set; }

        [Required, StringLength(120)]
        public string ContentType { get; set; }

        public long SizeBytes { get; set; }

        public int UploadedById { get; set; }
        public AccountUser UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
