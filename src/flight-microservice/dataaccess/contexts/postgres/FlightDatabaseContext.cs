using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;
using core.interfaces.dataaccess.contexts;

namespace dataaccess.contexts.postgres;

/// <summary>
/// EF Core DbContext for PostgreSQL database
/// Manages database connection and entity configurations
/// Implements IDatabaseContext for dependency inversion
/// </summary>
public class FlightDatabaseContext : DbContext, IDatabaseContext
{
    /// <summary>
    /// DbSet for Flight entities
    /// </summary>
    public virtual DbSet<FlightPostgresqlModel> Flights => Set<FlightPostgresqlModel>();

    /// <summary>
    /// DbSet for Airport entities
    /// </summary>
    public virtual DbSet<AirportPostgresqlModel> Airports => Set<AirportPostgresqlModel>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public FlightDatabaseContext() { }

    /// <summary>
    /// Constructor with DbContextOptions
    /// </summary>
    /// <param name="options">Entity Framework DbContext options</param>
    public FlightDatabaseContext(DbContextOptions<FlightDatabaseContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures model relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Flight entity
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

        // Configure Airport entity
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
    /// Configures database provider and connection string
    /// Reads connection parameters from environment variables
    /// </summary>
    /// <param name="optionsBuilder">DbContext options builder</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GetConnectionStringFromEnvironment();
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    /// <summary>
    /// Builds connection string from environment variables
    /// Uses standard PostgreSQL environment variables:
    /// - POSTGRESQL_HOST (default: localhost)
    /// - POSTGRESQL_PORT (default: 5432)
    /// - POSTGRESQL_DATABASE (default: flights)
    /// - POSTGRESQL_USER (default: program)
    /// - POSTGRESQL_PASSWORD (default: test)
    /// Or single POSTGRESQL_CONNECTION_STRING variable
    /// </summary>
    /// <returns>PostgreSQL connection string</returns>
    private static string GetConnectionStringFromEnvironment()
    {
        // Check for complete connection string first
        var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Build from individual environment variables
        var host = Environment.GetEnvironmentVariable("POSTGRESQL_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRESQL_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRESQL_DATABASE") ?? "flights";
        var username = Environment.GetEnvironmentVariable("POSTGRESQL_USER") ?? "program";
        var password = Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD") ?? "test";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task EnsureDatabaseCreatedAsync()
    {
        return Database.EnsureCreatedAsync();
    }
}
