using Microsoft.EntityFrameworkCore;
using SupportLink.Models;

namespace SupportLink.Data
{
    public class SupportLinkDbContext : DbContext
    {
        public SupportLinkDbContext(DbContextOptions<SupportLinkDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<AccountUser> Users { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketUpdate> TicketUpdates { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Organization> Organizations { get; set; }
       // public object Users { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AccountUser -> SupportTickets (created)
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.AccountUser)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AccountUser -> SupportTickets (assigned)
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Organization -> Tickets
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Organization)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // TicketUpdate -> Ticket
            modelBuilder.Entity<TicketUpdate>()
                .HasOne(u => u.Ticket)
                .WithMany(t => t.Updates)
                .HasForeignKey(u => u.SupportId)
                .OnDelete(DeleteBehavior.Cascade);

            // TicketUpdate -> AccountUser
            modelBuilder.Entity<TicketUpdate>()
                .HasOne(u => u.UpdatedBy)
                .WithMany(a => a.Updates)
                .HasForeignKey(u => u.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Feedback -> Ticket (1-to-1)
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Ticket)
                .WithOne(t => t.Feedback)
                .HasForeignKey<Feedback>(f => f.SupportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Feedback -> AccountUser
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Save UserRole enum as string instead of int
            modelBuilder.Entity<AccountUser>()
                .Property(u => u.Role)
                .HasConversion<string>();
        }
    }
}