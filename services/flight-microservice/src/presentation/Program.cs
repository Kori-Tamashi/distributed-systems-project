using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Configure PostgreSQL database context
var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING")
    ?? $"Host={Environment.GetEnvironmentVariable("POSTGRESQL_HOST") ?? "localhost"};"
        + $"Port={Environment.GetEnvironmentVariable("POSTGRESQL_PORT") ?? "5432"};"
        + $"Database={Environment.GetEnvironmentVariable("POSTGRESQL_DATABASE") ?? "flights"};"
        + $"Username={Environment.GetEnvironmentVariable("POSTGRESQL_USER") ?? "program"};"
        + $"Password={Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD") ?? "test"}";

builder.Services.AddDbContext<FlightDatabaseContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<IFlightRepository, FlightPostgresqlRepository>();
builder.Services.AddScoped<IAirportRepository, AirportPostgresqlRepository>();

// TODO: Register services after Business Logic Layer implementation
// builder.Services.AddScoped<IFlightService, FlightService>();
// builder.Services.AddScoped<IAirportService, AirportService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/manage/health");

app.Run();
