using Microsoft.AspNetCore.Mvc;

namespace presentation.dto.http;

/// <summary>
/// Base interface for HTTP-specific DTOs
/// </summary>
public interface IBaseHttpDto
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// HTTP status code associated with this DTO
    /// </summary>
    int? StatusCode { get; }
}

/// <summary>
/// Base DTO for HTTP responses with metadata
/// </summary>
public abstract class BaseHttpDto : IBaseHttpDto
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// HTTP status code associated with this DTO
    /// </summary>
    public int? StatusCode { get; protected set; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Request ID for tracing
    /// </summary>
    public string? RequestId { get; protected set; }

    /// <summary>
    /// Additional metadata for the response
    /// </summary>
    public Dictionary<string, object>? Metadata { get; protected set; }

    /// <summary>
    /// Adds metadata to the response
    /// </summary>
    protected void AddMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }

    /// <summary>
    /// Sets request ID for tracing
    /// </summary>
    protected void SetRequestId(string requestId)
    {
        RequestId = requestId;
    }

    /// <summary>
    /// Converts to ProblemDetails for error responses
    /// </summary>
    public virtual ProblemDetails? ToProblemDetails() => null;
}

/// <summary>
/// Base DTO for paginated HTTP responses
/// </summary>
public abstract class BasePagedHttpDto<TItem> : BaseHttpDto
{
    /// <summary>
    /// List of items
    /// </summary>
    public List<TItem> Items { get; set; } = new();

    /// <summary>
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (0-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 0;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages - 1;

    /// <summary>
    /// Sets pagination information
    /// </summary>
    protected void SetPagination(int page, int pageSize, int totalCount)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
