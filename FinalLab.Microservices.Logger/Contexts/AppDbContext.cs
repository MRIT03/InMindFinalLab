using FinalLab.Microservices.Logger.Entities;
using Microsoft.EntityFrameworkCore;


namespace FinalLab.Microservices.Logger.Contexts;
public class AppDbContext : DbContext
{
    public DbSet<LogEntry> Logs { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=LibraryLoggingDb;Username=postgres;Password=postgres");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogEntry>()
            .Property(l => l.Id)
            .UseIdentityAlwaysColumn();


        modelBuilder.Entity<LogEntry>()
            .Property(l => l.RequestId)
            .HasColumnType("uuid")
            .IsRequired();

        modelBuilder.Entity<LogEntry>()
            .Property(l => l.RequestObject)
            .HasColumnType("jsonb");

        modelBuilder.Entity<LogEntry>()
            .Property(l => l.RouteURL)
            .HasColumnType("text");

        modelBuilder.Entity<LogEntry>()
            .Property(l => l.Timestamp)
            .HasColumnType("timestamp");
        modelBuilder.Entity<LogEntry>()
            .HasIndex(l => l.RequestId)  // Index for request IDs
            .HasDatabaseName("IX_Logs_RequestId");
        modelBuilder.Entity<LogEntry>()
            .HasIndex(l => l.Timestamp)  // Index for timestamps
            .HasDatabaseName("IX_Logs_Timestamp");
    }
}