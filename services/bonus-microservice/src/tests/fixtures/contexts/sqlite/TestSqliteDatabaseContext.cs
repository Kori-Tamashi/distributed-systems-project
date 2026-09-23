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
    /// DbSet for Privilege entities
    /// </summary>
    public DbSet<PrivilegePostgresqlModel> Privileges => Set<PrivilegePostgresqlModel>();

    /// <summary>
    /// DbSet for PrivilegeHistory entities
    /// </summary>
    public DbSet<PrivilegeHistoryPostgresqlModel> PrivilegeHistories => Set<PrivilegeHistoryPostgresqlModel>();

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

        // Configure Privilege entity (same as production context)
        modelBuilder.Entity<PrivilegePostgresqlModel>(entity =>
        {
            entity.ToTable("privilege");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired();

            entity.Property(e => e.Balance)
                .HasColumnName("balance")
                .IsRequired();

            // Create indexes for faster searches
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // Configure PrivilegeHistory entity (same as production context)
        modelBuilder.Entity<PrivilegeHistoryPostgresqlModel>(entity =>
        {
            entity.ToTable("privilege_history");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.PrivilegeId)
                .HasColumnName("privilege_id")
                .IsRequired();

            entity.Property(e => e.TicketUid)
                .HasColumnName("ticket_uid")
                .IsRequired();

            entity.Property(e => e.DateTime)
                .HasColumnName("datetime")
                .IsRequired();

            entity.Property(e => e.BalanceDiff)
                .HasColumnName("balance_diff")
                .IsRequired();

            entity.Property(e => e.OperationType)
                .HasColumnName("operation_type")
                .IsRequired();

            // Create indexes for faster searches
            entity.HasIndex(e => e.PrivilegeId);
            entity.HasIndex(e => e.TicketUid);
            entity.HasIndex(e => e.DateTime);
            entity.HasIndex(e => e.OperationType);
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

        // Clear Privileges table
        await Privileges.ExecuteDeleteAsync();
        
        // Clear PrivilegeHistories table
        await PrivilegeHistories.ExecuteDeleteAsync();
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
