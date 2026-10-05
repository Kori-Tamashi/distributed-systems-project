using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess;
using core.interfaces.dataaccess.gateways;
using core.circuitbreaker;
using core.configuration;
using core.security;
using businesslogic.services;
using dataaccess.retry;
using dataaccess.gateways.http;
using dataaccess.gateways.circuitbreaker;
using presentation.controllers.http;
using presentation.security;
using presentation.middleware;
using presentation.http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Configuration
// ============================================
// 🚪 Gateway Microservice - Lab 02 - Flight Booking System
// ✅ Clean Architecture with HTTP Controllers and Gateways

// Load settings from environment variables
var apiTestSettings = LoadApiTestSettings();
var oidcSettings = LoadOidcSettings();

// Register Circuit Breaker states — one singleton per downstream service
builder.Services.AddKeyedSingleton<CircuitBreakerState>("flight",
    new CircuitBreakerState(failureThreshold: 3, probeInterval: TimeSpan.FromSeconds(3)));
builder.Services.AddKeyedSingleton<CircuitBreakerState>("ticket",
    new CircuitBreakerState(failureThreshold: 3, probeInterval: TimeSpan.FromSeconds(3)));
builder.Services.AddKeyedSingleton<CircuitBreakerState>("bonus",
    new CircuitBreakerState(failureThreshold: 3, probeInterval: TimeSpan.FromSeconds(3)));

// Register OIDC settings
builder.Services.AddSingleton(oidcSettings);

// Register IHttpContextAccessor for ICurrentUser
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Register JWT Bearer authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = oidcSettings.Issuer;
        options.RequireHttpsMetadata = false; // Development mode
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = oidcSettings.ValidIssuer,
            ValidateAudience = false, // ROPC tokens have aud="account", we don't check audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "preferred_username"
        };
    });
builder.Services.AddAuthorization();

// Retry queue for non-critical operations (Bonus rollback on ticket return)
builder.Services.AddSingleton<InMemoryRetryQueue>(sp =>
    new InMemoryRetryQueue(
        sp.GetRequiredService<ILogger<InMemoryRetryQueue>>(),
        retryDelay: TimeSpan.FromSeconds(10)));
builder.Services.AddSingleton<IRetryQueue>(sp => sp.GetRequiredService<InMemoryRetryQueue>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<InMemoryRetryQueue>());

// Register raw HTTP gateways (internal, wrapped by circuit breakers)
builder.Services.AddTransient<AirportHttpGateway>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("unnamed");
    return new AirportHttpGateway(httpClient, apiTestSettings.FlightMicroserviceUrl);
});

builder.Services.AddTransient<FlightHttpGateway>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("unnamed");
    return new FlightHttpGateway(httpClient, apiTestSettings.FlightMicroserviceUrl);
});

builder.Services.AddTransient<TicketHttpGateway>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("unnamed");
    return new TicketHttpGateway(httpClient, apiTestSettings.TicketMicroserviceUrl);
});

builder.Services.AddTransient<PrivilegeHttpGateway>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("unnamed");
    return new PrivilegeHttpGateway(httpClient, apiTestSettings.BonusMicroserviceUrl);
});

builder.Services.AddTransient<PrivilegeHistoryHttpGateway>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("unnamed");
    return new PrivilegeHistoryHttpGateway(httpClient, apiTestSettings.BonusMicroserviceUrl);
});

// Register decorated gateways exposed as I*Gateway interfaces
builder.Services.AddTransient<IAirportGateway>(provider =>
    new CircuitBreakerAirportGateway(
        provider.GetRequiredService<AirportHttpGateway>(),
        provider.GetRequiredKeyedService<CircuitBreakerState>("flight")));

builder.Services.AddTransient<IFlightGateway>(provider =>
    new CircuitBreakerFlightGateway(
        provider.GetRequiredService<FlightHttpGateway>(),
        provider.GetRequiredKeyedService<CircuitBreakerState>("flight")));

builder.Services.AddTransient<ITicketGateway>(provider =>
    new CircuitBreakerTicketGateway(
        provider.GetRequiredService<TicketHttpGateway>(),
        provider.GetRequiredKeyedService<CircuitBreakerState>("ticket")));

builder.Services.AddTransient<IPrivilegeGateway>(provider =>
    new CircuitBreakerPrivilegeGateway(
        provider.GetRequiredService<PrivilegeHttpGateway>(),
        provider.GetRequiredKeyedService<CircuitBreakerState>("bonus")));

builder.Services.AddTransient<IPrivilegeHistoryGateway>(provider =>
    provider.GetRequiredService<PrivilegeHistoryHttpGateway>()); // No CB for history

// Register Business Logic Services with dependency injection
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPrivilegeService, PrivilegeService>();
builder.Services.AddScoped<IPrivilegeHistoryService, PrivilegeHistoryService>();

// Register HTTP Controllers with dependency injection
builder.Services.AddScoped<AirportHttpController>();
builder.Services.AddScoped<FlightHttpController>();
builder.Services.AddScoped<TicketHttpController>();
builder.Services.AddScoped<PrivilegeHttpController>();
builder.Services.AddScoped<PrivilegeHistoryHttpController>();
builder.Services.AddScoped<UserHttpController>();
builder.Services.AddScoped<AuthorizeHttpController>();

// Add services to the container
builder.Services.AddSingleton(apiTestSettings);
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ForwardAuthHandler>();
builder.Services.AddHttpClient("unnamed")
    .AddHttpMessageHandler<ForwardAuthHandler>(); // Apply to all HttpClient instances
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Unique schemaId = full type name (handles types with same name in different namespaces)
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    
    // Resolve conflicting actions by taking the first match
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
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
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication(); // Standard JWT Bearer authentication
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

// Health check endpoint (no auth required)
app.MapHealthChecks("/manage/health").AllowAnonymous();

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
/// Loads OIDC settings from environment variables
/// </summary>
static OidcSettings LoadOidcSettings()
{
    var issuer = Environment.GetEnvironmentVariable("OIDC_ISSUER") 
                 ?? "http://localhost:8888/realms/rsoi";
    
    return new OidcSettings
    {
        Issuer = issuer,
        JwksUri = Environment.GetEnvironmentVariable("OIDC_JWKS_URI") 
                  ?? $"{issuer}/protocol/openid-connect/certs",
        TokenEndpoint = Environment.GetEnvironmentVariable("OIDC_TOKEN_ENDPOINT") 
                        ?? $"{issuer}/protocol/openid-connect/token",
        ClientId = Environment.GetEnvironmentVariable("OIDC_CLIENT_ID") ?? "gateway",
        ClientSecret = Environment.GetEnvironmentVariable("OIDC_CLIENT_SECRET") ?? "",
        ValidIssuer = issuer
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
