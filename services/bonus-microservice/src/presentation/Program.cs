using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using businesslogic.services;
using presentation.controllers.http;
using presentation.middleware;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Configuration
// ============================================
// 🎁 Bonus Microservice - Lab 02 - 2026-09-14
// ✅ Clean Architecture with HTTP Controllers
var appSettings = LoadAppSettings();

// Configure database context based on provider
BonusDatabaseContext databaseContext = CreateDatabaseContext(appSettings);

// Register services with dependency injection
builder.Services.AddSingleton(databaseContext);
builder.Services.AddScoped<IPrivilegeRepository>(provider =>
{
    var context = provider.GetRequiredService<BonusDatabaseContext>();
    var settings = provider.GetRequiredService<AppSettings>();
    
    return CreatePrivilegeRepository(context, settings);
});
builder.Services.AddScoped<IPrivilegeHistoryRepository>(provider =>
{
    var context = provider.GetRequiredService<BonusDatabaseContext>();
    var settings = provider.GetRequiredService<AppSettings>();
    
    return CreatePrivilegeHistoryRepository(context, settings);
});
builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
builder.Services.AddScoped<IPrivilegeHistoryService, PrivilegeHistoryService>();
builder.Services.AddScoped<PrivilegeHttpController>();
builder.Services.AddScoped<PrivilegeHistoryHttpController>();

// Add services to the container
builder.Services.AddSingleton(appSettings);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Configure Kestrel to use port from .env
var appUrl = $"http://{appSettings.Application.Host}:{appSettings.Application.Port}";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(appSettings.Application.Port);
});

var app = builder.Build();

// Configure HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

// Health check endpoint
app.MapHealthChecks("/manage/health");

app.UseAuthorization();
app.MapControllers();

// Ensure database is created on startup
await EnsureDatabaseCreatedAsync(databaseContext);

app.Run();

/// <summary>
/// Loads application settings from environment variables (.env file)
/// </summary>
static AppSettings LoadAppSettings()
{
    var provider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER")?.ToUpperInvariant();
    
    return new AppSettings
    {
        DatabaseProvider = provider switch
        {
            "POSTGRESQL" => DatabaseProvider.PostgreSQL,
            "MYSQL" => DatabaseProvider.MySQL,
            "SQLITE" => DatabaseProvider.SQLite,
            "SQLSERVER" => DatabaseProvider.SQLServer,
            _ => DatabaseProvider.PostgreSQL // Default
        },
        PostgreSQL = new PostgreSQLSettings
        {
            ConnectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING"),
            Host = Environment.GetEnvironmentVariable("POSTGRESQL_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRESQL_PORT") ?? "5432"),
            Database = Environment.GetEnvironmentVariable("POSTGRESQL_DATABASE") ?? "privileges",
            User = Environment.GetEnvironmentVariable("POSTGRESQL_USER") ?? "program",
            Password = Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD") ?? "test"
        },
        Application = new ApplicationSettings
        {
            Host = Environment.GetEnvironmentVariable("APP_HOST") ?? "0.0.0.0",
            Port = int.Parse(Environment.GetEnvironmentVariable("APP_PORT") ?? "8050"),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        }
    };
}

/// <summary>
/// Creates database context based on configured provider
/// </summary>
static BonusDatabaseContext CreateDatabaseContext(AppSettings settings)
{
    return settings.DatabaseProvider switch
    {
        DatabaseProvider.PostgreSQL => CreatePostgreSQLContext(settings.PostgreSQL),
        DatabaseProvider.MySQL => throw new NotImplementedException("MySQL context not yet implemented"),
        DatabaseProvider.SQLite => throw new NotImplementedException("SQLite context not yet implemented"),
        DatabaseProvider.SQLServer => throw new NotImplementedException("SQL Server context not yet implemented"),
        _ => throw new InvalidOperationException($"Unsupported database provider: {settings.DatabaseProvider}")
    };
}

/// <summary>
/// Creates PostgreSQL database context
/// </summary>
static BonusDatabaseContext CreatePostgreSQLContext(PostgreSQLSettings pgSettings)
{
    var connectionString = !string.IsNullOrEmpty(pgSettings.ConnectionString)
        ? pgSettings.ConnectionString
        : $"Host={pgSettings.Host};Port={pgSettings.Port};Database={pgSettings.Database};Username={pgSettings.User};Password={pgSettings.Password}";

    var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<BonusDatabaseContext>();
    optionsBuilder.UseNpgsql(connectionString);

    return new BonusDatabaseContext(optionsBuilder.Options);
}

/// <summary>
/// Creates PrivilegeRepository based on database provider
/// </summary>
static IPrivilegeRepository CreatePrivilegeRepository(BonusDatabaseContext context, AppSettings settings)
{
    return settings.DatabaseProvider switch
    {
        DatabaseProvider.PostgreSQL => CreatePostgreSQLPrivilegeRepository(context),
        DatabaseProvider.MySQL => throw new NotImplementedException("MySQL repository not yet implemented"),
        DatabaseProvider.SQLite => throw new NotImplementedException("SQLite repository not yet implemented"),
        DatabaseProvider.SQLServer => throw new NotImplementedException("SQL Server repository not yet implemented"),
        _ => throw new InvalidOperationException($"Unsupported database provider: {settings.DatabaseProvider}")
    };
}

/// <summary>
/// Creates PostgreSQL Privilege repository
/// </summary>
static IPrivilegeRepository CreatePostgreSQLPrivilegeRepository(BonusDatabaseContext context)
{
    return new PrivilegePostgresqlRepository(context);
}

/// <summary>
/// Creates PrivilegeHistoryRepository based on database provider
/// </summary>
static IPrivilegeHistoryRepository CreatePrivilegeHistoryRepository(BonusDatabaseContext context, AppSettings settings)
{
    return settings.DatabaseProvider switch
    {
        DatabaseProvider.PostgreSQL => CreatePostgreSQLPrivilegeHistoryRepository(context),
        DatabaseProvider.MySQL => throw new NotImplementedException("MySQL repository not yet implemented"),
        DatabaseProvider.SQLite => throw new NotImplementedException("SQLite repository not yet implemented"),
        DatabaseProvider.SQLServer => throw new NotImplementedException("SQL Server repository not yet implemented"),
        _ => throw new InvalidOperationException($"Unsupported database provider: {settings.DatabaseProvider}")
    };
}

/// <summary>
/// Creates PostgreSQL PrivilegeHistory repository
/// </summary>
static IPrivilegeHistoryRepository CreatePostgreSQLPrivilegeHistoryRepository(BonusDatabaseContext context)
{
    return new PrivilegeHistoryPostgresqlRepository(context);
}

/// <summary>
/// Ensures database is created on application startup
/// </summary>
static async Task EnsureDatabaseCreatedAsync(BonusDatabaseContext context)
{
    try
    {
        await context.EnsureDatabaseCreatedAsync();
        Console.WriteLine("Database created successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not create database: {ex.Message}");
    }
}

/// <summary>
/// Application settings
/// </summary>
class AppSettings
{
    public DatabaseProvider DatabaseProvider { get; set; }
    public PostgreSQLSettings PostgreSQL { get; set; }
    public ApplicationSettings Application { get; set; }
}

/// <summary>
/// Database provider enumeration
/// </summary>
enum DatabaseProvider
{
    PostgreSQL,
    MySQL,
    SQLite,
    SQLServer
}

/// <summary>
/// PostgreSQL-specific settings
/// </summary>
class PostgreSQLSettings
{
    public string? ConnectionString { get; set; }
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "privileges";
    public string User { get; set; } = "program";
    public string Password { get; set; } = "test";
}

/// <summary>
/// Application-wide settings
/// </summary>
class ApplicationSettings
{
    public string Host { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 8050;
    public string Environment { get; set; } = "Development";
}
