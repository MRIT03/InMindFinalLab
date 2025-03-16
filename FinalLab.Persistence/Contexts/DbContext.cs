using FinalLab.Domain.Entities;
using FinalLab.Domain.Entities.Events.UpdateEvents;
using FinalLab.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FinalLab.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        { 
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        // Single table (TPH) for both Account and Transaction update events
        public DbSet<UpdateEvent> Events { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.AccountId);
                entity.Property(a => a.AccountId).ValueGeneratedOnAdd();
                entity.Property(a => a.Balance)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.Property(t => t.TransactionType)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(t => t.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
                entity.Property(t => t.Timestamp)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(t => t.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(t => t.Details)
                      .HasMaxLength(500);

                entity.HasOne<Account>()
                      .WithMany()
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UpdateEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId).ValueGeneratedOnAdd();
                entity.Property(e => e.OldStatus)
                      .HasMaxLength(50);
                entity.Property(e => e.NewStatus)
                      .HasMaxLength(50);
                entity.Property(e => e.OldBalance)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.NewBalance)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Timestamp)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure the self-referencing relationship for revert events.
                // If confused about this check the UpdateEvent.cs file for thorough explanation
                entity.HasOne(e => e.ParentEvent)
                      .WithMany(e => e.RevertEvents)
                      .HasForeignKey(e => e.ParentEventId)
                      .OnDelete(DeleteBehavior.Restrict);

                // TPH: Configure discriminator column
                // Events for transactions and accoutns will be stored in the same db table
                entity.HasDiscriminator<string>("EventType")
                      .HasValue<AccountUpdateEvent>("AccountUpdate")
                      .HasValue<TransactionUpdateEvent>("TransactionUpdate");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
