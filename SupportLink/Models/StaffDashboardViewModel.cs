using System.Collections.Generic;

namespace SupportLink.Models
{
	public class StaffDashboardViewModel
	{
		public int MyOpenCount { get; set; }
		public int MySolvedCount { get; set; }
		public int MyTotalCount { get; set; }

		public int MyNewCount { get; set; }
		public int MyAssignedCount { get; set; }
		public int MyInProgressCount { get; set; }
		public int MyResolvedCount { get; set; }
		public int MyClosedCount { get; set; }

		public int MyUpdatesCount { get; set; }
		public int MyFeedbackCount { get; set; }

		public IList<SupportTicket> RecentTickets { get; set; } = new List<SupportTicket>();
		public IList<SupportTicket> MySolvedTickets { get; set; } = new List<SupportTicket>();
		public IList<SupportTicket> MyUnsolvedTickets { get; set; } = new List<SupportTicket>();
	}
}


