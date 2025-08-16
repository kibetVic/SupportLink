using Microsoft.EntityFrameworkCore;

namespace SupportLink.Models
{
    public class SupportLinkDbContext : DbContext
    {
        public SupportLinkDbContext(DbContextOptions<SupportLinkDbContext> options)
            : base(options)
        {
        }

        public DbSet<AccountUser> ApplicationUsers { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketUpdate> TicketUpdates { get; set; }
        public DbSet<Feedback> Feedback { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(s => s.AccountUser)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Keep cascade on primary user

            modelBuilder.Entity<SupportTicket>()
                .HasOne(s => s.AssignedAgent)
                .WithMany()
                .HasForeignKey(s => s.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict); // Change cascade to Restrict


            modelBuilder.Entity<TicketUpdate>()
                .HasOne(tu => tu.UpdatedBy)
                .WithMany()
                .HasForeignKey(tu => tu.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // SupportTicket to Feedback (keep cascade)
            //modelBuilder.Entity<SupportTicket>()
            //    .HasOne(t => t.Feedback)
            //    .WithOne(f => f.Ticket)
            //    .HasForeignKey<Feedback>(f => f.SupportId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // SupportTicket to AssignedAgent (optional)
            //modelBuilder.Entity<SupportTicket>()
            //    .HasOne(s => s.AssignedAgent)
            //    .WithMany(u => u.AssignedTickets)
            //    .HasForeignKey(s => s.AssignedAgentId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
