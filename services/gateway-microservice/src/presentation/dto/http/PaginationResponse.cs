using System.Text.Json.Serialization;

namespace presentation.dto.http;

/// <summary>
/// Paginated response wrapper (per lab2-template v1 spec)
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
public class PaginationResponse<T>
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }

    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

    public PaginationResponse() { }

    public PaginationResponse(List<T> items, int page, int pageSize, int totalElements)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalElements = totalElements;
    }
}
