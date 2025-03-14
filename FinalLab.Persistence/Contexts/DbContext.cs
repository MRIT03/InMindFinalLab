using FinalLab.Domain.Entities;
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
        public DbSet<UpdateEvent> UpdateEvents { get; set; } // TPH Table for both Account & Transaction Events

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.AccountId);
                entity.Property(a => a.AccountId).ValueGeneratedOnAdd();
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)").IsRequired();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedOnAdd();
                entity.Property(t => t.TransactionType).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(t => t.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(t => t.Status).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Details).HasMaxLength(500);

                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UpdateEvent>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventId).ValueGeneratedOnAdd();
                entity.Property(e => e.oldStatus).HasMaxLength(50);
                entity.Property(e => e.newStatus).HasMaxLength(50);
                entity.Property(e => e.oldBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.newBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.timeStamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // TPH: Discriminator Column
                entity.HasDiscriminator<string>("EventType")
                      .HasValue<AccountUpdateEvent>("AccountUpdate")
                      .HasValue<TransactionUpdateEvent>("TransactionUpdate");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
