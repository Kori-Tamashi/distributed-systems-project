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
    /// DbSet for Person entities
    /// </summary>
    public DbSet<PersonPostgresqlModel> Persons => Set<PersonPostgresqlModel>();

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

        // Configure Person entity (same as production context)
        modelBuilder.Entity<PersonPostgresqlModel>(entity =>
        {
            entity.ToTable("persons");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Age)
                .HasColumnName("age")
                .IsRequired(false);

            entity.Property(e => e.Address)
                .HasColumnName("address")
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(e => e.Work)
                .HasColumnName("work")
                .HasMaxLength(255)
                .IsRequired(false);

            // Create index on Name for faster searches
            entity.HasIndex(e => e.Name);
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

        // Clear Persons table
        await Persons.ExecuteDeleteAsync();
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
