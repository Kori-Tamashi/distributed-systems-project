using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

namespace tests.fixtures.contexts.sqlite;

/// <summary>
/// Test SQLite database context for unit tests
/// Uses in-memory database for fast, isolated testing
/// Note: True in-memory SQLite database per test instance
/// </summary>
public class TestSqliteDatabaseContext : DbContext
{
    /// <summary>
    /// DbSet for Ticket entities
    /// </summary>
    public DbSet<TicketPostgresqlModel> Tickets => Set<TicketPostgresqlModel>();

    /// <summary>
    /// Default constructor - creates in-memory database
    /// </summary>
    public TestSqliteDatabaseContext()
    {
        Database.EnsureCreated();
    }

    /// <summary>
    /// Constructor with DbContextOptions
    /// </summary>
    /// <param name="options">Entity Framework DbContext options</param>
    public TestSqliteDatabaseContext(DbContextOptions<TestSqliteDatabaseContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    /// <summary>
    /// Configures model relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Ticket entity (same as production context)
        modelBuilder.Entity<TicketPostgresqlModel>(entity =>
        {
            entity.ToTable("tickets");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TicketUid)
                .HasColumnName("ticket_uid")
                .IsRequired();

            entity.Property(e => e.FlightNumber)
                .HasColumnName("flight_id")
                .IsRequired();

            entity.Property(e => e.Username)
                .HasColumnName("passenger_name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Username)
                .HasColumnName("passenger_email")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Username)
                .HasColumnName("passenger_phone")
                .HasMaxLength(50);

            entity.Property(e => e.FlightNumber)
                .HasColumnName("seat_number")
                .HasMaxLength(10);

            entity.Property(e => e.Status)
                .HasColumnName("class")
                .IsRequired();

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .IsRequired();

            entity.Property(e => e.FlightNumber)
                .HasColumnName("booking_date")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired();

            // Create indexes for faster searches
            entity.HasIndex(e => e.TicketUid);
            entity.HasIndex(e => e.FlightNumber);
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FlightNumber);
        });
    }

    /// <summary>
    /// Configures database provider to use in-memory SQLite
    /// Each instance gets a separate in-memory database
    /// </summary>
    /// <param name="optionsBuilder">DbContext options builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use in-memory SQLite database
            // Note: Each DbContext instance gets its own separate database
            optionsBuilder.UseSqlite("Data Source=:memory:");
        }
    }

    /// <summary>
    /// Ensures database is created
    /// Already called in constructor, but can be called explicitly
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }

    /// <summary>
    /// Clears all data from all tables
    /// Useful for test cleanup without recreating database
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        // Disable auto-tracking for bulk delete
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        // Clear Tickets table
        await Tickets.ExecuteDeleteAsync();
    }

    /// <summary>
    /// Creates options for a new context with shared in-memory database
    /// Use this for multiple contexts sharing the same database
    /// </summary>
    /// <returns>DbContextOptions configured for in-memory SQLite</returns>
    public static DbContextOptions<TestSqliteDatabaseContext> CreateInMemoryOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestSqliteDatabaseContext>();
        optionsBuilder.UseSqlite("Data Source=:memory:");
        return optionsBuilder.Options;
    }
}
