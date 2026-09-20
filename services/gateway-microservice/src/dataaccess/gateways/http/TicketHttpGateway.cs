using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Ticket;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for Ticket microservice
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

        if (filter.FlightId.HasValue)
            parameters.Add($"flightId={filter.FlightId.Value}");
        if (!string.IsNullOrEmpty(filter.PassengerName))
            parameters.Add($"passengerName={Uri.EscapeDataString(filter.PassengerName)}");
        if (!string.IsNullOrEmpty(filter.PassengerEmail))
            parameters.Add($"passengerEmail={Uri.EscapeDataString(filter.PassengerEmail)}");
        if (!string.IsNullOrEmpty(filter.SeatNumber))
            parameters.Add($"seatNumber={Uri.EscapeDataString(filter.SeatNumber)}");
        if (filter.Class.HasValue)
            parameters.Add($"class={(int)filter.Class.Value}");
        if (filter.MinPrice.HasValue)
            parameters.Add($"minPrice={filter.MinPrice.Value}");
        if (filter.MaxPrice.HasValue)
            parameters.Add($"maxPrice={filter.MaxPrice.Value}");
        if (filter.MinBookingDate.HasValue)
            parameters.Add($"minBookingDate={filter.MinBookingDate.Value:yyyy-MM-dd}");
        if (filter.MaxBookingDate.HasValue)
            parameters.Add($"maxBookingDate={filter.MaxBookingDate.Value:yyyy-MM-dd}");
        if (filter.Status.HasValue)
            parameters.Add($"status={(int)filter.Status.Value}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
