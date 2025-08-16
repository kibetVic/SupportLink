using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportLink.Models
{
    public class AccountUser
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // Customer, Agent, Admin
        //public ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
       // public ICollection<SupportTicket> AssignedTickets { get; set; } = new List<SupportTicket>();
    }
    public enum Role
    {
        Admin,
        SupperAdmin,
        Agent,
        Staff
    }
}
