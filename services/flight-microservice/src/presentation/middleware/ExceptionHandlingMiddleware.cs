using System.Net;
using System.Text.Json;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.repositories;

namespace presentation.middleware;

/// <summary>
/// Global exception handling middleware for flight-microservice
/// Maps domain exceptions to appropriate HTTP status codes and JSON responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred while processing the request");

        var response = context.Response;
        response.ContentType = "application/json";

        object errorResponse;

        switch (exception)
        {
            case ServiceEntityNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new { message = exception.Message };
                break;

            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    message = exception.Message,
                    errors = validationEx.Errors?.SelectMany(kvp => kvp.Value).ToArray() ?? Array.Empty<string>()
                };
                break;

            case BusinessRuleViolationException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse = new { message = exception.Message };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new { message = "An internal server error occurred" };
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(errorResponse, options);
        return response.WriteAsync(json);
    }
}
