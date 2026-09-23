using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http.Flight;
using Microsoft.Extensions.Logging;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for Flight microservice
/// Communicates with Flight microservice via REST API
/// Implements best practices: logging, error handling, timeout support
/// </summary>
public class FlightHttpGateway : BaseHttpGateway, IFlightGateway
{
    private const string ApiEndpoint = "/api/v1/flights";

    /// <summary>
    /// Initializes a new instance of FlightHttpGateway
    /// </summary>
    /// <param name="httpClient">HttpClient configured for Flight microservice</param>
    /// <param name="baseUrl">Base URL of Flight microservice API (e.g., http://localhost:8060/api/v1)</param>
    /// <param name="logger">Optional logger for diagnostic information</param>
    public FlightHttpGateway(HttpClient httpClient, string baseUrl, ILogger<FlightHttpGateway>? logger = null)
        : base(httpClient, baseUrl, logger)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Flight>> GetAllAsync()
    {
        Logger?.LogInformation("Fetching all flights from Flight microservice");
        var dtos = await GetAsync<IEnumerable<FlightDTO>>(ApiEndpoint);
        var result = dtos?.Select(FlightHttpConverter.ToDomain) ?? Enumerable.Empty<Flight>();
        Logger?.LogInformation("Retrieved {Count} flights", result.Count());
        return result;
    }

    /// <inheritdoc />
    public async Task<Flight?> GetByIdAsync(int id)
    {
        Logger?.LogInformation("Fetching flight with id {Id} from Flight microservice", id);
        try
        {
            var dto = await GetAsync<FlightDTO>($"{ApiEndpoint}/{id}");
            var result = dto != null ? FlightHttpConverter.ToDomain(dto) : null;
            return result;
        }
        catch (GatewayEntityNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Flight>> GetAllAsync(FlightFilter filter)
    {
        var queryString = BuildFilterQueryString(filter);
        var dtos = await GetAsync<IEnumerable<FlightDTO>>($"{ApiEndpoint}{queryString}");
        
        var result = dtos?.Select(FlightHttpConverter.ToDomain) ?? Enumerable.Empty<Flight>();
        return result;
    }

    /// <inheritdoc />
    public async Task<Flight> CreateAsync(Flight flight)
    {
        var dto = FlightHttpConverter.ToDTO(flight);
        var createdDto = await PostAsync<CreateFlightDTO>($"{ApiEndpoint}", dto);
        var result = FlightHttpConverter.ToDomain(createdDto);
        return result;
    }

    /// <inheritdoc />
    public async Task<Flight> UpdateAsync(Flight flight)
    {
        var dto = FlightHttpConverter.ToUpdateDTO(flight);
        var updatedDto = await PutAsync<UpdateFlightDTO>($"{ApiEndpoint}/{flight.Id}", dto);
        var result = FlightHttpConverter.ToDomain(updatedDto);
        return result;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        await DeleteAsync($"{ApiEndpoint}/{id}");
    }

    /// <inheritdoc />
    protected override Exception CreateNotFoundException(string entityName, int? entityId)
    {
        return entityId.HasValue
            ? new FlightGatewayEntityNotFoundException($"{entityName} with id {entityId} not found")
            : new FlightGatewayEntityNotFoundException($"{entityName} not found");
    }

    /// <summary>
    /// Builds query string from filter
    /// </summary>
    private string BuildFilterQueryString(FlightFilter filter)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(filter.FlightNumber))
            parameters.Add($"flightNumber={Uri.EscapeDataString(filter.FlightNumber)}");
        if (filter.FromAirportId.HasValue)
            parameters.Add($"fromAirportId={filter.FromAirportId.Value}");
        if (filter.ToAirportId.HasValue)
            parameters.Add($"toAirportId={filter.ToAirportId.Value}");
        if (filter.MinPrice.HasValue)
            parameters.Add($"minPrice={filter.MinPrice.Value}");
        if (filter.MaxPrice.HasValue)
            parameters.Add($"maxPrice={filter.MaxPrice.Value}");
        if (filter.MinDateTime.HasValue)
            parameters.Add($"minDateTime={filter.MinDateTime.Value:yyyy-MM-dd}");
        if (filter.MaxDateTime.HasValue)
            parameters.Add($"maxDateTime={filter.MaxDateTime.Value:yyyy-MM-dd}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
