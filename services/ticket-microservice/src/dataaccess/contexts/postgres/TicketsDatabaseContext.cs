using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using core.interfaces.dataaccess.contexts;

namespace dataaccess.contexts.postgres;

/// <summary>
/// EF Core DbContext for PostgreSQL database (per lab2-template v1 spec)
/// Table: ticket (singular)
/// Columns: id, ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketsDatabaseContext : DbContext, IDatabaseContext
{
    /// <summary>
    /// DbSet for Ticket entities (table name: ticket - singular per spec)
    /// </summary>
    public virtual DbSet<TicketPostgresqlModel> Ticket => Set<TicketPostgresqlModel>();

    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Gets the database connection string
    /// </summary>
    public string ConnectionString => GetConnectionStringFromEnvironment();

    /// <summary>
    /// Default constructor
    /// </summary>
    public TicketsDatabaseContext() { }

    /// <summary>
    /// Constructor with DbContextOptions
    /// </summary>
    /// <param name="options">Entity Framework DbContext options</param>
    public TicketsDatabaseContext(DbContextOptions<TicketsDatabaseContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures model relationships and constraints (per lab2-template v1 spec)
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Ticket entity (table: ticket - singular)
        modelBuilder.Entity<TicketPostgresqlModel>(entity =>
        {
            entity.ToTable("ticket");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TicketUid)
                .HasColumnName("ticket_uid")
                .IsRequired();

            entity.Property(e => e.Username)
                .HasColumnName("username")
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(e => e.FlightNumber)
                .HasColumnName("flight_number")
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired();

            // Create indexes for faster searches
            entity.HasIndex(e => e.TicketUid).IsUnique();
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.FlightNumber);
            entity.HasIndex(e => e.Status);
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
    /// - POSTGRESQL_DATABASE (default: tickets)
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
        var database = Environment.GetEnvironmentVariable("POSTGRESQL_DATABASE") ?? "tickets";
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

    /// <inheritdoc/>
    public Task OpenAsync()
    {
        return Database.OpenConnectionAsync();
    }

    /// <inheritdoc/>
    public void Close()
    {
        Database.CloseConnection();
    }

    /// <inheritdoc/>
    public Task BeginTransactionAsync()
    {
        _transaction = Database.BeginTransaction();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
        return Task.CompletedTask;
    }
}
