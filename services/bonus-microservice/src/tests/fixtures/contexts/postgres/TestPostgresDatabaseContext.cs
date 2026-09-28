using System;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

namespace tests.fixtures.contexts.postgres;

/// <summary>
/// Test PostgreSQL database context for integration tests
/// Uses TEST_* environment variables for connection configuration
/// </summary>
public class TestPostgresDatabaseContext : BonusDatabaseContext
{
    /// <summary>
    /// DbSet for Privilege entities
    /// </summary>
    public new DbSet<PrivilegePostgresqlModel> Privileges => Set<PrivilegePostgresqlModel>();

    /// <summary>
    /// DbSet for PrivilegeHistory entities
    /// </summary>
    public new DbSet<PrivilegeHistoryPostgresqlModel> PrivilegeHistories => Set<PrivilegeHistoryPostgresqlModel>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public TestPostgresDatabaseContext() { }

    /// <summary>
    /// Ensures database is deleted (for test isolation)
    /// </summary>
    public void EnsureDatabaseDeleted()
    {
        Database.EnsureDeleted();
    }

    /// <summary>
    /// Ensures database is created (for test initialization)
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }

    /// <summary>
    /// Gets connection string from TEST_* environment variables
    /// Override parent method to use test-specific variables
    /// </summary>
    /// <returns>Test PostgreSQL connection string</returns>
    private static string GetConnectionStringFromEnvironment()
    {
        // Check for complete test connection string first
        var connectionString = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Build from individual test environment variables
        var host = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_PORT") ?? "5452";
        var database = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_DATABASE") ?? "test_bonuses";
        var username = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_USER") ?? "program";
        var password = Environment.GetEnvironmentVariable("TEST_POSTGRESQL_PASSWORD") ?? "test";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    /// <summary>
    /// Connection string property - uses test-specific connection string
    /// </summary>
    public new string ConnectionString => GetConnectionStringFromEnvironment();

    /// <summary>
    /// Configures database connection for test PostgreSQL
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
    /// Clears all data from all tables
    /// Useful for test cleanup without dropping database
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
}
