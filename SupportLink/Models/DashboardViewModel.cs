using SupportLink.Models;

namespace SupportLink.Models
{
    public class DashboardViewModel
    {
        public int UnAssignedCount { get; set; }

        public int NewCount { get; set; }
        public int AssignedCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
        public int SolvedCount { get; set; }
        public int UnsolvedCount { get; set; }
        public int TotalTickets { get; set; }

        // Lists for tables on dashboard
        public IList<SupportTicket> UnAssignedTickets { get; set; } = new List<SupportTicket>();
        public IList<SupportTicket> SolvedTickets { get; set; } = new List<SupportTicket>();
        public IList<SupportTicket> UnsolvedTickets { get; set; } = new List<SupportTicket>();
    }
}

