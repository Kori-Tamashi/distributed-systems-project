using core.domain;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.dataaccess.gateways;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Booking;

namespace dataaccess.gateways.http;

/// <summary>
/// HTTP Gateway implementation for Booking API within Ticket microservice
/// </summary>
public class BookingHttpGateway : BaseHttpGateway, IBookingGateway
{
    private const string ApiEndpoint = "/api/v1/bookings";

    public BookingHttpGateway(HttpClient httpClient, string baseUrl)
        : base(httpClient, baseUrl)
    {
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        var dtos = await GetAsync<IEnumerable<BookingDTO>>(ApiEndpoint);
        return dtos?.Select(BookingHttpConverter.ToDomain) ?? Enumerable.Empty<Booking>();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        try
        {
            var dto = await GetAsync<BookingDTO>($"{ApiEndpoint}/{id}");
            return dto != null ? BookingHttpConverter.ToDomain(dto) : null;
        }
        catch (GatewayEntityNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(BookingFilter filter)
    {
        var queryString = BuildFilterQueryString(filter);
        var dtos = await GetAsync<IEnumerable<BookingDTO>>($"{ApiEndpoint}{queryString}");
        return dtos?.Select(BookingHttpConverter.ToDomain) ?? Enumerable.Empty<Booking>();
    }

    public async Task<Booking> CreateAsync(Booking booking)
    {
        var dto = BookingHttpConverter.ToDTO(booking);
        var createdDto = await PostAsync<CreateBookingDTO>($"{ApiEndpoint}", dto);
        return BookingHttpConverter.ToDomain(createdDto);
    }

    public async Task<Booking> UpdateAsync(Booking booking)
    {
        var dto = BookingHttpConverter.ToUpdateDTO(booking);
        var updatedDto = await PutAsync<UpdateBookingDTO>($"{ApiEndpoint}/{booking.Id}", dto);
        return BookingHttpConverter.ToDomain(updatedDto);
    }

    public async Task DeleteAsync(int id)
    {
        await DeleteAsync($"{ApiEndpoint}/{id}");
    }

    protected override Exception CreateNotFoundException(string entityName, int? entityId)
    {
        return entityId.HasValue
            ? new BookingGatewayEntityNotFoundException($"{entityName} with id {entityId} not found")
            : new BookingGatewayEntityNotFoundException($"{entityName} not found");
    }

    private string BuildFilterQueryString(BookingFilter filter)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(filter.BookingReference))
            parameters.Add($"bookingReference={Uri.EscapeDataString(filter.BookingReference)}");
        if (!string.IsNullOrEmpty(filter.CustomerEmail))
            parameters.Add($"customerEmail={Uri.EscapeDataString(filter.CustomerEmail)}");
        if (!string.IsNullOrEmpty(filter.CustomerName))
            parameters.Add($"customerName={Uri.EscapeDataString(filter.CustomerName)}");
        if (filter.MinTotalPrice.HasValue)
            parameters.Add($"minTotalPrice={filter.MinTotalPrice.Value}");
        if (filter.MaxTotalPrice.HasValue)
            parameters.Add($"maxTotalPrice={filter.MaxTotalPrice.Value}");
        if (filter.MinBookingDate.HasValue)
            parameters.Add($"minBookingDate={filter.MinBookingDate.Value:yyyy-MM-dd}");
        if (filter.MaxBookingDate.HasValue)
            parameters.Add($"maxBookingDate={filter.MaxBookingDate.Value:yyyy-MM-dd}");
        if (filter.Status.HasValue)
            parameters.Add($"status={(int)filter.Status.Value}");
        if (filter.PaymentMethod.HasValue)
            parameters.Add($"paymentMethod={(int)filter.PaymentMethod.Value}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }
}
