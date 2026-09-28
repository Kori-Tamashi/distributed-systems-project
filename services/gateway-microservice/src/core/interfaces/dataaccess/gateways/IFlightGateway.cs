using core.domain;
using core.filters;
using core.exceptions.dataaccess.gateways;

namespace core.interfaces.dataaccess.gateways;

/// <summary>
/// Gateway interface for Flight microservice communication.
/// Provides methods to interact with Flight microservice via HTTP API.
/// This gateway acts as a client for the Flight microservice, abstracting
/// the HTTP communication details from the business logic layer.
/// </summary>
public interface IFlightGateway
{
    /// <summary>
    /// Retrieves all flights from the Flight microservice.
    /// </summary>
    /// <returns>A collection of all flights.</returns>
    /// <exception cref="FlightGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="FlightGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="FlightGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<IEnumerable<Flight>> GetAllAsync();

    /// <summary>
    /// Retrieves a flight by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the flight.</param>
    /// <returns>The flight entity, or <c>null</c> if not found.</returns>
    /// <exception cref="FlightGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="FlightGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="FlightGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<Flight?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves flights filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for querying flights.</param>
    /// <returns>A collection of flights matching the filter criteria.</returns>
    /// <exception cref="FlightGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="FlightGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="FlightGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<IEnumerable<Flight>> GetAllAsync(FlightFilter filter);

    /// <summary>
    /// Creates a new flight in the Flight microservice.
    /// </summary>
    /// <param name="flight">The flight entity to create.</param>
    /// <returns>The created flight with the generated identifier.</returns>
    /// <exception cref="FlightGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="FlightGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="FlightGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<Flight> CreateAsync(Flight flight);

    /// <summary>
    /// Updates an existing flight in the Flight microservice.
    /// </summary>
    /// <param name="flight">The flight entity to update.</param>
    /// <returns>The updated flight.</returns>
    /// <exception cref="FlightGatewayEntityNotFoundException">Thrown when the flight with specified ID is not found.</exception>
    /// <exception cref="FlightGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="FlightGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="FlightGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<Flight> UpdateAsync(Flight flight);

    /// <summary>
    /// Deletes a flight by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the flight to delete.</param>
    /// <returns><c>true</c> if the flight was successfully deleted; <c>false</c> if the flight was not found.</returns>
    /// <exception cref="FlightGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="FlightGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="FlightGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task DeleteAsync(int id);
}
