using core.domain;
using core.filters;
using core.exceptions.dataaccess.gateways;

namespace core.interfaces.dataaccess.gateways;

/// <summary>
/// Gateway interface for Booking communication within Ticket microservice.
/// Provides methods to interact with Booking API of Ticket microservice via HTTP.
/// This gateway abstracts the HTTP communication details from the business logic layer.
/// </summary>
public interface IBookingGateway
{
    /// <summary>
    /// Retrieves all bookings from the Ticket microservice.
    /// </summary>
    /// <returns>A collection of all bookings.</returns>
    /// <exception cref="BookingGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="BookingGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="BookingGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<IEnumerable<Booking>> GetAllAsync();

    /// <summary>
    /// Retrieves a booking by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the booking.</param>
    /// <returns>The booking entity, or <c>null</c> if not found.</returns>
    /// <exception cref="BookingGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="BookingGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="BookingGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<Booking?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves bookings filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for querying bookings.</param>
    /// <returns>A collection of bookings matching the filter criteria.</returns>
    /// <exception cref="BookingGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="BookingGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="BookingGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<IEnumerable<Booking>> GetAllAsync(BookingFilter filter);

    /// <summary>
    /// Creates a new booking in the Ticket microservice.
    /// </summary>
    /// <param name="booking">The booking entity to create.</param>
    /// <returns>The created booking with the generated identifier.</returns>
    /// <exception cref="BookingGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="BookingGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="BookingGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<Booking> CreateAsync(Booking booking);

    /// <summary>
    /// Updates an existing booking in the Ticket microservice.
    /// </summary>
    /// <param name="booking">The booking entity to update.</param>
    /// <returns>The updated booking.</returns>
    /// <exception cref="BookingGatewayEntityNotFoundException">Thrown when the booking with specified ID is not found.</exception>
    /// <exception cref="BookingGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="BookingGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="BookingGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<Booking> UpdateAsync(Booking booking);

    /// <summary>
    /// Deletes a booking by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the booking to delete.</param>
    /// <returns><c>true</c> if the booking was successfully deleted; <c>false</c> if the booking was not found.</returns>
    /// <exception cref="BookingGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="BookingGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="BookingGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task DeleteAsync(int id);
}
