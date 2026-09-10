using Microsoft.AspNetCore.Mvc;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.repositories;
using presentation.exceptions;

namespace presentation.exceptions.http;

/// <summary>
/// HTTP controller exception for Person Validation Errors (400)
/// Maps PersonValidationException to HTTP ProblemDetails
/// </summary>
public class HttpPersonValidationException : BaseHttpControllerException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public HttpPersonValidationException(PersonValidationException exception)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status400BadRequest,
            errorCode: "PERSON_VALIDATION_FAILED",
            type: "https://httpstatuses.com/400",
            title: "Person Validation Failed",
            detail: exception.Message)
    {
        ValidationErrors = exception.Data.Count > 0 
            ? (Dictionary<string, string[]>)exception.Data 
            : new Dictionary<string, string[]>();
    }

    public HttpPersonValidationException(PersonValidationException exception, Exception innerException)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status400BadRequest,
            errorCode: "PERSON_VALIDATION_FAILED",
            type: "https://httpstatuses.com/400",
            title: "Person Validation Failed",
            detail: exception.Message,
            innerException: innerException)
    {
        ValidationErrors = exception.Data.Count > 0 
            ? (Dictionary<string, string[]>)exception.Data 
            : new Dictionary<string, string[]>();
    }

    public override ProblemDetails ToProblemDetails()
    {
        var problemDetails = base.ToProblemDetails();
        if (ValidationErrors.Count > 0)
        {
            problemDetails.Extensions["errors"] = ValidationErrors;
        }
        return problemDetails;
    }
}

/// <summary>
/// HTTP controller exception for Person Already Exists (409)
/// Maps PersonAlreadyExistsException to HTTP ProblemDetails
/// </summary>
public class HttpPersonAlreadyExistsException : BaseHttpControllerException
{
    public int PersonId { get; }

    public HttpPersonAlreadyExistsException(PersonAlreadyExistsException exception)
        : base(
            message: $"Person with ID {exception.PersonId} already exists",
            statusCode: StatusCodes.Status409Conflict,
            errorCode: "PERSON_ALREADY_EXISTS",
            type: "https://httpstatuses.com/409",
            title: "Person Already Exists",
            detail: $"Person with ID {exception.PersonId} already exists")
    {
        PersonId = exception.PersonId;
    }

    public HttpPersonAlreadyExistsException(PersonAlreadyExistsException exception, Exception innerException)
        : base(
            message: $"Person with ID {exception.PersonId} already exists",
            statusCode: StatusCodes.Status409Conflict,
            errorCode: "PERSON_ALREADY_EXISTS",
            type: "https://httpstatuses.com/409",
            title: "Person Already Exists",
            detail: $"Person with ID {exception.PersonId} already exists",
            innerException: innerException)
    {
        PersonId = exception.PersonId;
    }
}

/// <summary>
/// HTTP controller exception for Person Business Rule Violation (422)
/// Maps PersonBusinessRuleViolationException to HTTP ProblemDetails
/// </summary>
public class HttpPersonBusinessRuleViolationException : BaseHttpControllerException
{
    public string RuleName { get; }

    public HttpPersonBusinessRuleViolationException(PersonBusinessRuleViolationException exception)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status422UnprocessableEntity,
            errorCode: "PERSON_BUSINESS_RULE_VIOLATION",
            type: "https://httpstatuses.com/422",
            title: "Person Business Rule Violation",
            detail: exception.Message)
    {
        RuleName = exception.RuleName;
    }

    public HttpPersonBusinessRuleViolationException(PersonBusinessRuleViolationException exception, Exception innerException)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status422UnprocessableEntity,
            errorCode: "PERSON_BUSINESS_RULE_VIOLATION",
            type: "https://httpstatuses.com/422",
            title: "Person Business Rule Violation",
            detail: exception.Message,
            innerException: innerException)
    {
        RuleName = exception.RuleName;
    }
}

/// <summary>
/// HTTP controller exception for Person Database Error (500)
/// Maps PersonDatabaseException to HTTP ProblemDetails
/// </summary>
public class HttpPersonDatabaseException : BaseHttpControllerException
{
    public HttpPersonDatabaseException(PersonDatabaseException exception)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            errorCode: "PERSON_DATABASE_ERROR",
            type: "https://httpstatuses.com/500",
            title: "Person Database Error",
            detail: "An error occurred while processing your request")
    {
    }

    public HttpPersonDatabaseException(PersonDatabaseException exception, Exception innerException)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            errorCode: "PERSON_DATABASE_ERROR",
            type: "https://httpstatuses.com/500",
            title: "Person Database Error",
            detail: "An error occurred while processing your request",
            innerException: innerException)
    {
    }
}

/// <summary>
/// HTTP controller exception for Generic Person Errors (500)
/// Maps BaseServiceException to HTTP ProblemDetails
/// </summary>
public class HttpPersonServiceException : BaseHttpControllerException
{
    public HttpPersonServiceException(BaseServiceException exception)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            errorCode: "PERSON_SERVICE_ERROR",
            type: "https://httpstatuses.com/500",
            title: "Person Service Error",
            detail: "An error occurred in the Person service")
    {
    }

    public HttpPersonServiceException(BaseServiceException exception, Exception innerException)
        : base(
            message: exception.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            errorCode: "PERSON_SERVICE_ERROR",
            type: "https://httpstatuses.com/500",
            title: "Person Service Error",
            detail: "An error occurred in the Person service",
            innerException: innerException)
    {
    }
}
