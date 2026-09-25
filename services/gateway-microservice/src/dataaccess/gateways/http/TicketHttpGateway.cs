using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http.Ticket;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for Ticket microservice (per lab2-template v1 spec)
/// </summary>
public class TicketHttpGateway : BaseHttpGateway, ITicketGateway
{
    private const string ApiEndpoint = "/api/v1/tickets";

    public TicketHttpGateway(HttpClient httpClient, string baseUrl)
        : base(httpClient, baseUrl)
    {
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        var dtos = await GetAsync<IEnumerable<TicketDTO>>(ApiEndpoint);
        return dtos?.Select(TicketHttpConverter.ToDomain) ?? Enumerable.Empty<Ticket>();
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<TicketDTO>($"{ApiEndpoint}/{id}");
            return dto != null ? TicketHttpConverter.ToDomain(dto) : null;
        }
        catch (GatewayEntityNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync(TicketFilter filter)
    {
        var queryString = BuildFilterQueryString(filter);
        var dtos = await GetAsync<IEnumerable<TicketDTO>>($"{ApiEndpoint}{queryString}");
        return dtos?.Select(TicketHttpConverter.ToDomain) ?? Enumerable.Empty<Ticket>();
    }

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        var dto = TicketHttpConverter.ToCreateDTO(ticket);
        var createdDto = await PostAsync<CreateTicketDTO>($"{ApiEndpoint}", dto);
        return TicketHttpConverter.ToDomain(createdDto);
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        var dto = TicketHttpConverter.ToUpdateDTO(ticket);
        var updatedDto = await PutAsync<UpdateTicketDTO>($"{ApiEndpoint}/{ticket.Id}", dto);
        return TicketHttpConverter.ToDomain(updatedDto);
    }

    public async Task DeleteAsync(int id)
    {
        await DeleteAsync($"{ApiEndpoint}/{id}");
    }

    protected override Exception CreateNotFoundException(string entityName, int? entityId)
    {
        return entityId.HasValue
            ? new TicketGatewayEntityNotFoundException($"{entityName} with id {entityId} not found")
            : new TicketGatewayEntityNotFoundException($"{entityName} not found");
    }

    private string BuildFilterQueryString(TicketFilter filter)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(filter.FlightNumber))
            parameters.Add($"flightNumber={Uri.EscapeDataString(filter.FlightNumber)}");
        if (!string.IsNullOrEmpty(filter.Username))
            parameters.Add($"username={Uri.EscapeDataString(filter.Username)}");
        if (filter.MinPrice.HasValue)
            parameters.Add($"minPrice={filter.MinPrice.Value}");
        if (filter.MaxPrice.HasValue)
            parameters.Add($"maxPrice={filter.MaxPrice.Value}");
        if (filter.Status.HasValue)
            parameters.Add($"status={filter.Status.Value}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
