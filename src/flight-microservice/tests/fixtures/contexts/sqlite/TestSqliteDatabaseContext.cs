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
    /// DbSet for Flight entities
    /// </summary>
    public DbSet<FlightPostgresqlModel> Flights => Set<FlightPostgresqlModel>();

    /// <summary>
    /// DbSet for Airport entities
    /// </summary>
    public DbSet<AirportPostgresqlModel> Airports => Set<AirportPostgresqlModel>();

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

        // Configure Flight entity (same as production context)
        modelBuilder.Entity<FlightPostgresqlModel>(entity =>
        {
            entity.ToTable("flights");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.FlightUid)
                .HasColumnName("flight_uid")
                .IsRequired();

            entity.Property(e => e.FlightNumber)
                .HasColumnName("flight_number")
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.DateTime)
                .HasColumnName("datetime")
                .IsRequired();

            entity.Property(e => e.FromAirportId)
                .HasColumnName("from_airport_id")
                .IsRequired();

            entity.Property(e => e.ToAirportId)
                .HasColumnName("to_airport_id")
                .IsRequired();

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .IsRequired();

            // Configure relationships
            entity.HasOne(e => e.FromAirport)
                .WithMany(a => a.DepartingFlights)
                .HasForeignKey(e => e.FromAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToAirport)
                .WithMany(a => a.ArrivingFlights)
                .HasForeignKey(e => e.ToAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create indexes for faster searches
            entity.HasIndex(e => e.FlightNumber);
            entity.HasIndex(e => e.FromAirportId);
            entity.HasIndex(e => e.ToAirportId);
            entity.HasIndex(e => e.DateTime);
        });

        // Configure Airport entity (same as production context)
        modelBuilder.Entity<AirportPostgresqlModel>(entity =>
        {
            entity.ToTable("airports");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(255);

            entity.Property(e => e.City)
                .HasColumnName("city")
                .HasMaxLength(255);

            entity.Property(e => e.Country)
                .HasColumnName("country")
                .HasMaxLength(255);

            // Create indexes for faster searches
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Country);
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

        // Clear Flights table
        await Flights.ExecuteDeleteAsync();
        
        // Clear Airports table
        await Airports.ExecuteDeleteAsync();
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
