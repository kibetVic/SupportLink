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

        // 🔹 DbSets
        public DbSet<AccountUser> Users { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketUpdate> TicketUpdates { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<IssueCategory> IssueCategories { get; set; }
        public DbSet<FileType> FileTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 AccountUser -> SupportTickets (Created by)
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.AccountUser)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 AccountUser -> SupportTickets (Assigned to)
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.AssignedAgent)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedAgentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Organization -> Tickets
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Organization)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Ticket -> Status
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Status)
                .WithMany(s => s.SupportTickets)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Ticket -> IssueCategory
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.IssueCategory)
                .WithMany(c => c.SupportTickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 TicketUpdate -> Ticket
            modelBuilder.Entity<TicketUpdate>()
                .HasOne(u => u.Ticket)
                .WithMany(t => t.Updates)
                .HasForeignKey(u => u.SupportId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 TicketUpdate -> AccountUser
            modelBuilder.Entity<TicketUpdate>()
                .HasOne(u => u.UpdatedBy)
                .WithMany(a => a.Updates)
                .HasForeignKey(u => u.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Feedback -> Ticket (1-to-1)
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Ticket)
                .WithOne(t => t.Feedback)
                .HasForeignKey<Feedback>(f => f.SupportId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Feedback -> AccountUser
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Save UserRole enum as string instead of int
            modelBuilder.Entity<AccountUser>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // ✅ Seed TicketStatuses
            modelBuilder.Entity<TicketStatus>().HasData(
                new TicketStatus { StatusId = 1, Name = "New", Description = "Ticket just created" },
                new TicketStatus { StatusId = 2, Name = "Assigned", Description = "Assigned to an agent" },
                new TicketStatus { StatusId = 3, Name = "InProgress", Description = "Work in progress" },
                new TicketStatus { StatusId = 4, Name = "Resolved", Description = "Issue resolved" },
                new TicketStatus { StatusId = 5, Name = "Closed", Description = "Ticket closed" }
            );

            // ✅ Seed IssueCategories
            modelBuilder.Entity<IssueCategory>().HasData(
                new IssueCategory { CategoryId = 1, Name = "Technical", Description = "Technical issues" },
                new IssueCategory { CategoryId = 2, Name = "Billing", Description = "Billing and payments" },
                new IssueCategory { CategoryId = 3, Name = "Account", Description = "Account-related issues" },
                new IssueCategory { CategoryId = 4, Name = "General Inquiry", Description = "General questions" },
                new IssueCategory { CategoryId = 5, Name = "Other", Description = "Other types of issues" }
            );

            modelBuilder.Entity<FileType>().HasData(
                new FileType { FileTypeId = 1, Name = "Image" },
                new FileType { FileTypeId = 2, Name = "Document" },
                new FileType { FileTypeId = 3, Name = "PDF" },
                new FileType { FileTypeId = 4, Name = "Video" },
                new FileType { FileTypeId = 5, Name = "Others" }
            );
        }
    }
}
