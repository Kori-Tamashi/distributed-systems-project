using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using businesslogic.services;
using dataaccess.gateways.http;
using presentation.controllers.http;
using presentation.controllers.http;
using System.Text.Json;
using System.Text.Json.Serialization;

// Load .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Configuration
// ============================================
// 🚪 Gateway Microservice - Lab 02 - Flight Booking System
// ✅ Clean Architecture with HTTP Controllers and Gateways

// Load settings from environment variables
var apiTestSettings = LoadApiTestSettings();

// Register HTTP Gateways with dependency injection
builder.Services.AddTransient<IAirportGateway>(provider =>
{
    var httpClient = new HttpClient();
    return new AirportHttpGateway(httpClient, apiTestSettings.FlightMicroserviceUrl);
});

builder.Services.AddTransient<IFlightGateway>(provider =>
{
    var httpClient = new HttpClient();
    return new FlightHttpGateway(httpClient, apiTestSettings.FlightMicroserviceUrl);
});

builder.Services.AddTransient<ITicketGateway>(provider =>
{
    var httpClient = new HttpClient();
    return new TicketHttpGateway(httpClient, apiTestSettings.TicketMicroserviceUrl);
});

builder.Services.AddTransient<IBookingGateway>(provider =>
{
    var httpClient = new HttpClient();
    return new BookingHttpGateway(httpClient, apiTestSettings.TicketMicroserviceUrl);
});

builder.Services.AddTransient<IPrivilegeGateway>(provider =>
{
    var httpClient = new HttpClient();
    return new PrivilegeHttpGateway(httpClient, apiTestSettings.BonusMicroserviceUrl);
});

builder.Services.AddTransient<IPrivilegeHistoryGateway>(provider =>
{
    var httpClient = new HttpClient();
    return new PrivilegeHistoryHttpGateway(httpClient, apiTestSettings.BonusMicroserviceUrl);
});

// Register Business Logic Services with dependency injection
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
builder.Services.AddScoped<IPrivilegeHistoryService, PrivilegeHistoryService>();

// Register SAGA Coordinators
builder.Services.AddScoped<BookingSagaCoordinator>();

// Register HTTP Controllers with dependency injection
builder.Services.AddScoped<AirportHttpController>();
builder.Services.AddScoped<FlightHttpController>();
builder.Services.AddScoped<TicketHttpController>();
builder.Services.AddScoped<BookingHttpController>();
builder.Services.AddScoped<PrivilegeHttpController>();
builder.Services.AddScoped<PrivilegeHistoryHttpController>();

// Add services to the container
builder.Services.AddSingleton(apiTestSettings);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Configure Kestrel to use port from .env
var appSettings = LoadApplicationSettings();
var appUrl = $"http://{appSettings.Host}:{appSettings.Port}";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(appSettings.Port);
});

var app = builder.Build();

// Configure HTTP request pipeline - Swagger enabled for all environments
app.UseSwagger();
app.UseSwaggerUI();

// Health check endpoint
app.MapHealthChecks("/health");

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("============================================");
Console.WriteLine("🚪 Gateway Microservice Started");
Console.WriteLine("============================================");
Console.WriteLine($"📍 URL: http://localhost:{appSettings.Port}");
Console.WriteLine($"📍 Swagger: http://localhost:{appSettings.Port}/swagger");
Console.WriteLine($"📍 Health: http://localhost:{appSettings.Port}/health");
Console.WriteLine("============================================");
Console.WriteLine($"✈️  Flight API: {apiTestSettings.FlightMicroserviceUrl}");
Console.WriteLine($"🎫 Ticket API: {apiTestSettings.TicketMicroserviceUrl}");
Console.WriteLine($"🎁 Bonus API: {apiTestSettings.BonusMicroserviceUrl}");
Console.WriteLine("============================================");

app.Run();

/// <summary>
/// Loads API test settings from environment variables
/// </summary>
static ApiTestSettings LoadApiTestSettings()
{
    return new ApiTestSettings
    {
        FlightMicroserviceUrl = Environment.GetEnvironmentVariable("FLIGHT_API_URL") 
                                ?? "http://host.docker.internal:8060",
        TicketMicroserviceUrl = Environment.GetEnvironmentVariable("TICKET_API_URL") 
                                ?? "http://host.docker.internal:8070",
        BonusMicroserviceUrl = Environment.GetEnvironmentVariable("BONUS_API_URL") 
                               ?? "http://host.docker.internal:8050"
    };
}

/// <summary>
/// Loads application settings from environment variables
/// </summary>
static ApplicationSettings LoadApplicationSettings()
{
    return new ApplicationSettings
    {
        Host = Environment.GetEnvironmentVariable("APP_HOST") ?? "0.0.0.0",
        Port = int.Parse(Environment.GetEnvironmentVariable("APP_PORT") ?? "8080"),
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
    };
}

/// <summary>
/// API Test Settings for inter-service communication
/// </summary>
class ApiTestSettings
{
    public string FlightMicroserviceUrl { get; set; } = "http://host.docker.internal:8060";
    public string TicketMicroserviceUrl { get; set; } = "http://host.docker.internal:8070";
    public string BonusMicroserviceUrl { get; set; } = "http://host.docker.internal:8050";
}

/// <summary>
/// Application-wide settings
/// </summary>
class ApplicationSettings
{
    public string Host { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 8080;
    public string Environment { get; set; } = "Development";
}
