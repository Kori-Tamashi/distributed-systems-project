using core.domain;
using core.filters;
using core.exceptions.dataaccess.gateways;

namespace core.interfaces.dataaccess.gateways;

/// <summary>
/// Gateway interface for Airport communication within Flight microservice.
/// Provides methods to interact with Airport API of Flight microservice via HTTP.
/// This gateway abstracts the HTTP communication details from the business logic layer.
/// </summary>
public interface IAirportGateway
{
    /// <summary>
    /// Retrieves all airports from the Flight microservice.
    /// </summary>
    /// <returns>A collection of all airports.</returns>
    /// <exception cref="AirportGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="AirportGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="AirportGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<IEnumerable<Airport>> GetAllAsync();

    /// <summary>
    /// Retrieves an airport by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the airport.</param>
    /// <returns>The airport entity, or <c>null</c> if not found.</returns>
    /// <exception cref="AirportGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="AirportGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="AirportGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<Airport?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves airports filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for querying airports.</param>
    /// <returns>A collection of airports matching the filter criteria.</returns>
    /// <exception cref="AirportGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="AirportGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="AirportGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<IEnumerable<Airport>> GetAllAsync(AirportFilter filter);

    /// <summary>
    /// Creates a new airport in the Flight microservice.
    /// </summary>
    /// <param name="airport">The airport entity to create.</param>
    /// <returns>The created airport with the generated identifier.</returns>
    /// <exception cref="AirportGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="AirportGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="AirportGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<Airport> CreateAsync(Airport airport);

    /// <summary>
    /// Updates an existing airport in the Flight microservice.
    /// </summary>
    /// <param name="airport">The airport entity to update.</param>
    /// <returns>The updated airport.</returns>
    /// <exception cref="AirportGatewayEntityNotFoundException">Thrown when the airport with specified ID is not found.</exception>
    /// <exception cref="AirportGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="AirportGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="AirportGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task<Airport> UpdateAsync(Airport airport);

    /// <summary>
    /// Deletes an airport by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the airport to delete.</param>
    /// <returns><c>true</c> if the airport was successfully deleted; <c>false</c> if the airport was not found.</returns>
    /// <exception cref="AirportGatewayCommunicationException">Thrown when communication with Flight microservice fails.</exception>
    /// <exception cref="AirportGatewayTimeoutException">Thrown when the request to Flight microservice times out.</exception>
    /// <exception cref="AirportGatewayInternalServerException">Thrown when Flight microservice returns an internal server error.</exception>
    Task DeleteAsync(int id);
}
