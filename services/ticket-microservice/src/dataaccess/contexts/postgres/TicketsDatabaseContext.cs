using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using core.interfaces.dataaccess.contexts;

namespace dataaccess.contexts.postgres;

/// <summary>
/// EF Core DbContext for PostgreSQL database
/// Manages database connection and entity configurations
/// Implements IDatabaseContext for dependency inversion
/// </summary>
public class TicketsDatabaseContext : DbContext, IDatabaseContext
{
    /// <summary>
    /// DbSet for Ticket entities
    /// </summary>
    public virtual DbSet<TicketPostgresqlModel> Tickets => Set<TicketPostgresqlModel>();

    /// <summary>
    /// DbSet for Booking entities
    /// </summary>
    public virtual DbSet<BookingPostgresqlModel> Bookings => Set<BookingPostgresqlModel>();

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
    /// Configures model relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Ticket entity
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

            entity.Property(e => e.FlightId)
                .HasColumnName("flight_id")
                .IsRequired();

            entity.Property(e => e.PassengerName)
                .HasColumnName("passenger_name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PassengerEmail)
                .HasColumnName("passenger_email")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.PassengerPhone)
                .HasColumnName("passenger_phone")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.SeatNumber)
                .HasColumnName("seat_number")
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.Class)
                .HasColumnName("class")
                .IsRequired();

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .IsRequired();

            entity.Property(e => e.BookingDate)
                .HasColumnName("booking_date")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired();

            // Create indexes for faster searches
            entity.HasIndex(e => e.TicketUid);
            entity.HasIndex(e => e.FlightId);
            entity.HasIndex(e => e.PassengerEmail);
            entity.HasIndex(e => e.BookingDate);
            entity.HasIndex(e => e.Status);
        });

        // Configure Booking entity
        modelBuilder.Entity<BookingPostgresqlModel>(entity =>
        {
            entity.ToTable("bookings");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.BookingUid)
                .HasColumnName("booking_uid")
                .IsRequired();

            entity.Property(e => e.BookingReference)
                .HasColumnName("booking_reference")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CustomerName)
                .HasColumnName("customer_name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CustomerEmail)
                .HasColumnName("customer_email")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CustomerPhone)
                .HasColumnName("customer_phone")
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TotalPrice)
                .HasColumnName("total_price")
                .IsRequired();

            entity.Property(e => e.BookingDate)
                .HasColumnName("booking_date")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .IsRequired();

            entity.Property(e => e.PaymentMethod)
                .HasColumnName("payment_method")
                .IsRequired();

            entity.Property(e => e.PaymentTransactionId)
                .HasColumnName("payment_transaction_id")
                .HasMaxLength(255);

            // Create indexes for faster searches
            entity.HasIndex(e => e.BookingUid);
            entity.HasIndex(e => e.BookingReference);
            entity.HasIndex(e => e.CustomerEmail);
            entity.HasIndex(e => e.BookingDate);
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
