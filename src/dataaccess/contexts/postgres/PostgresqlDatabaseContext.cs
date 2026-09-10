using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;
using core.interfaces.dataaccess.contexts;

namespace dataaccess.contexts.postgres;

/// <summary>
/// EF Core DbContext for PostgreSQL database
/// Manages database connection and entity configurations
/// Implements IDatabaseContext for dependency inversion
/// </summary>
public class PostgresqlDatabaseContext : DbContext, IDatabaseContext
{
    /// <summary>
    /// DbSet for Person entities
    /// </summary>
    public virtual DbSet<PersonPostgresqlModel> Persons => Set<PersonPostgresqlModel>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public PostgresqlDatabaseContext() { }

    /// <summary>
    /// Constructor with DbContextOptions
    /// </summary>
    /// <param name="options">Entity Framework DbContext options</param>
    public PostgresqlDatabaseContext(DbContextOptions<PostgresqlDatabaseContext> options)
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

        // Configure Person entity
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
    /// - POSTGRESQL_DATABASE (default: persons)
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
        var database = Environment.GetEnvironmentVariable("POSTGRESQL_DATABASE") ?? "persons";
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
