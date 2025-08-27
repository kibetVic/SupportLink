using System.Collections.Generic;

namespace SupportLink.Models
{
	public class AgentDashboardViewModel
	{
		public int AssignedOpen { get; set; }
		public int NewlyAssigned { get; set; }
		public int InProgress { get; set; }
		public int Solved { get; set; }
		public int UpdatesMade { get; set; }

		public IList<SupportTicket> RecentAssignedTickets { get; set; } = new List<SupportTicket>();
	}
}


