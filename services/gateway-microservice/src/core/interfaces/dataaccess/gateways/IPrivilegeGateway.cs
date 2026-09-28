using core.domain;
using core.filters;
using core.exceptions.dataaccess.gateways;

namespace core.interfaces.dataaccess.gateways;

/// <summary>
/// Gateway interface for Privilege communication within Bonus microservice.
/// Provides methods to interact with Privilege API of Bonus microservice via HTTP.
/// This gateway abstracts the HTTP communication details from the business logic layer.
/// </summary>
public interface IPrivilegeGateway
{
    /// <summary>
    /// Retrieves all privileges from the Bonus microservice.
    /// </summary>
    /// <returns>A collection of all privileges.</returns>
    /// <exception cref="PrivilegeGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<IEnumerable<Privilege>> GetAllAsync();

    /// <summary>
    /// Retrieves a privilege by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege.</param>
    /// <returns>The privilege entity, or <c>null</c> if not found.</returns>
    /// <exception cref="PrivilegeGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<Privilege?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves privileges filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for querying privileges.</param>
    /// <returns>A collection of privileges matching the filter criteria.</returns>
    /// <exception cref="PrivilegeGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<IEnumerable<Privilege>> GetAllAsync(PrivilegeFilter filter);

    /// <summary>
    /// Creates a new privilege in the Bonus microservice.
    /// </summary>
    /// <param name="privilege">The privilege entity to create.</param>
    /// <returns>The created privilege with the generated identifier.</returns>
    /// <exception cref="PrivilegeGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<Privilege> CreateAsync(Privilege privilege);

    /// <summary>
    /// Updates an existing privilege in the Bonus microservice.
    /// </summary>
    /// <param name="privilege">The privilege entity to update.</param>
    /// <returns>The updated privilege.</returns>
    /// <exception cref="PrivilegeGatewayEntityNotFoundException">Thrown when the privilege with specified ID is not found.</exception>
    /// <exception cref="PrivilegeGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<Privilege> UpdateAsync(Privilege privilege);

    /// <summary>
    /// Deletes a privilege by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege to delete.</param>
    /// <returns><c>true</c> if the privilege was successfully deleted; <c>false</c> if the privilege was not found.</returns>
    /// <exception cref="PrivilegeGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task DeleteAsync(int id);
}
