using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

namespace tests.fixtures.contexts.postgres;

/// <summary>
/// Test PostgreSQL database context for integration tests
/// Uses TEST_* environment variables for connection configuration
/// </summary>
public class TestPostgresqlDatabaseContext : PostgresqlDatabaseContext
{
    /// <summary>
    /// DbSet for Person entities
    /// </summary>
    public new DbSet<PersonPostgresqlModel> Persons => Set<PersonPostgresqlModel>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public TestPostgresqlDatabaseContext() { }

    /// <summary>
    /// Constructor with DbContextOptions
    /// </summary>
    /// <param name="options">Entity Framework DbContext options</param>
    public TestPostgresqlDatabaseContext(DbContextOptions<PostgresqlDatabaseContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures database provider and connection string
    /// Reads connection parameters from TEST_* environment variables
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
    /// Builds connection string from TEST_* environment variables
    /// Uses standard PostgreSQL environment variables with TEST_ prefix:
    /// - TEST_POSTGRESQL_CONNECTION_STRING (complete connection string)
    /// - TEST_POSTGRESQL_HOST (default: localhost)
    /// - TEST_POSTGRESQL_PORT (default: 5432)
    /// - TEST_POSTGRESQL_DATABASE (default: test_persons)
    /// - TEST_POSTGRESQL_USER (default: program)
    /// - TEST_POSTGRESQL_PASSWORD (default: test)
    /// </summary>
    /// <returns>PostgreSQL connection string for testing</returns>
    private static string GetConnectionStringFromEnvironment()
    {
        // Check for complete connection string first
        var connectionString = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Build from individual environment variables with TEST_ prefix
        var host = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_DATABASE") ?? "test_persons";
        var username = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_USER") ?? "program";
        var password = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_PASSWORD") ?? "test";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    /// <summary>
    /// Ensures database is created before tests run
    /// Call this in test setup
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }

    /// <summary>
    /// Drops and recreates the database
    /// Use with caution in tests
    /// </summary>
    public void EnsureDatabaseDeleted()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
    }
}
