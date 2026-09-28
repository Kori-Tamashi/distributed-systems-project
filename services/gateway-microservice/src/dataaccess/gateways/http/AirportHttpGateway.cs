using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Airport;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for Airport API within Flight microservice
/// </summary>
public class AirportHttpGateway : BaseHttpGateway, IAirportGateway
{
    private const string ApiEndpoint = "/api/v1/airports";

    public AirportHttpGateway(HttpClient httpClient, string baseUrl)
        : base(httpClient, baseUrl)
    {
    }

    public async Task<IEnumerable<Airport>> GetAllAsync()
    {
        var dtos = await GetAsync<IEnumerable<AirportDTO>>(ApiEndpoint);
        return dtos?.Select(AirportHttpConverter.ToDomain) ?? Enumerable.Empty<Airport>();
    }

    public async Task<Airport?> GetByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<AirportDTO>($"{ApiEndpoint}/{id}");
            return dto != null ? AirportHttpConverter.ToDomain(dto) : null;
        }
        catch (GatewayEntityNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Airport>> GetAllAsync(AirportFilter filter)
    {
        var queryString = BuildFilterQueryString(filter);
        var dtos = await GetAsync<IEnumerable<AirportDTO>>($"{ApiEndpoint}{queryString}");
        return dtos?.Select(AirportHttpConverter.ToDomain) ?? Enumerable.Empty<Airport>();
    }

    public async Task<Airport> CreateAsync(Airport airport)
    {
        var dto = AirportHttpConverter.ToDTO(airport);
        var createdDto = await PostAsync<CreateAirportDTO>($"{ApiEndpoint}", dto);
        return AirportHttpConverter.ToDomain(createdDto);
    }

    public async Task<Airport> UpdateAsync(Airport airport)
    {
        var dto = AirportHttpConverter.ToUpdateDTO(airport);
        var updatedDto = await PutAsync<UpdateAirportDTO>($"{ApiEndpoint}/{airport.Id}", dto);
        return AirportHttpConverter.ToDomain(updatedDto);
    }

    public async Task DeleteAsync(int id)
    {
        await DeleteAsync($"{ApiEndpoint}/{id}");
    }

    protected override Exception CreateNotFoundException(string entityName, int? entityId)
    {
        return entityId.HasValue
            ? new AirportGatewayEntityNotFoundException($"{entityName} with id {entityId} not found")
            : new AirportGatewayEntityNotFoundException($"{entityName} not found");
    }

    private string BuildFilterQueryString(AirportFilter filter)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(filter.Name))
            parameters.Add($"name={Uri.EscapeDataString(filter.Name)}");
        if (!string.IsNullOrEmpty(filter.City))
            parameters.Add($"city={Uri.EscapeDataString(filter.City)}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
