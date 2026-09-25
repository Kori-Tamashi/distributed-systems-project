using System.Net;
using System.Text.Json;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using Microsoft.AspNetCore.Mvc;
using presentation.dto.http;
using presentation.exceptions.http;

namespace presentation.middleware;

/// <summary>
/// Global exception handling middleware for gateway-microservice
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred while processing the request");

        var response = context.Response;
        response.ContentType = "application/json";

        object errorResponse;
        int statusCode;

        if (exception is ValidationException v)
        {
            var errors = v.Errors?.SelectMany(kvp => kvp.Value).ToList() ?? new List<string>();
            errorResponse = new ValidationErrorResponse(v.Message, errors);
            statusCode = (int)HttpStatusCode.BadRequest;
        }
        else if (exception is presentation.exceptions.http.Ticket.TicketValidationException ve)
        {
            var errors = ve.Errors?.SelectMany(kvp => kvp.Value).ToList() ?? new List<string>();
            errorResponse = new ValidationErrorResponse(ve.Message, errors);
            statusCode = (int)HttpStatusCode.BadRequest;
        }
        else if (exception is BaseHttpException be && be.ErrorData is { Count: > 0 })
        {
            var errors = be.ErrorData.SelectMany(kvp => kvp.Value).ToList();
            errorResponse = new ValidationErrorResponse(be.Message, errors);
            statusCode = be.StatusCode;
        }
        else if (exception is BaseHttpException be2)
        {
            errorResponse = new ErrorResponse(be2.Message);
            statusCode = be2.StatusCode;
        }
        else if (exception is ServiceEntityNotFoundException)
        {
            errorResponse = new ErrorResponse(exception.Message);
            statusCode = (int)HttpStatusCode.NotFound;
        }
        else if (exception is BusinessRuleViolationException)
        {
            errorResponse = new ErrorResponse(exception.Message);
            statusCode = (int)HttpStatusCode.Conflict;
        }
        else
        {
            errorResponse = new ErrorResponse("An internal server error occurred");
            statusCode = (int)HttpStatusCode.InternalServerError;
        }

        response.StatusCode = statusCode;
        await response.WriteAsJsonAsync(errorResponse);
    }
}
