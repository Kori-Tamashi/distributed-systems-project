using core.domain;
using core.filters;
using core.exceptions.dataaccess.gateways;

namespace core.interfaces.dataaccess.gateways;

/// <summary>
/// Gateway interface for PrivilegeHistory communication within Bonus microservice.
/// Provides methods to interact with PrivilegeHistory API of Bonus microservice via HTTP.
/// This gateway abstracts the HTTP communication details from the business logic layer.
/// </summary>
public interface IPrivilegeHistoryGateway
{
    /// <summary>
    /// Retrieves all privilege histories from the Bonus microservice.
    /// </summary>
    /// <returns>A collection of all privilege histories.</returns>
    /// <exception cref="PrivilegeHistoryGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeHistoryGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeHistoryGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<IEnumerable<PrivilegeHistory>> GetAllAsync();

    /// <summary>
    /// Retrieves a privilege history by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege history.</param>
    /// <returns>The privilege history entity, or <c>null</c> if not found.</returns>
    /// <exception cref="PrivilegeHistoryGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeHistoryGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeHistoryGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<PrivilegeHistory?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves privilege histories filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for querying privilege histories.</param>
    /// <returns>A collection of privilege histories matching the filter criteria.</returns>
    /// <exception cref="PrivilegeHistoryGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeHistoryGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeHistoryGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<IEnumerable<PrivilegeHistory>> GetAllAsync(PrivilegeHistoryFilter filter);

    /// <summary>
    /// Creates a new privilege history in the Bonus microservice.
    /// </summary>
    /// <param name="privilegeHistory">The privilege history entity to create.</param>
    /// <returns>The created privilege history with the generated identifier.</returns>
    /// <exception cref="PrivilegeHistoryGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeHistoryGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeHistoryGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<PrivilegeHistory> CreateAsync(PrivilegeHistory privilegeHistory);

    /// <summary>
    /// Updates an existing privilege history in the Bonus microservice.
    /// </summary>
    /// <param name="privilegeHistory">The privilege history entity to update.</param>
    /// <returns>The updated privilege history.</returns>
    /// <exception cref="PrivilegeHistoryGatewayEntityNotFoundException">Thrown when the privilege history with specified ID is not found.</exception>
    /// <exception cref="PrivilegeHistoryGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeHistoryGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeHistoryGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task<PrivilegeHistory> UpdateAsync(PrivilegeHistory privilegeHistory);

    /// <summary>
    /// Deletes a privilege history by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the privilege history to delete.</param>
    /// <returns><c>true</c> if the privilege history was successfully deleted; <c>false</c> if the privilege history was not found.</returns>
    /// <exception cref="PrivilegeHistoryGatewayCommunicationException">Thrown when communication with Bonus microservice fails.</exception>
    /// <exception cref="PrivilegeHistoryGatewayTimeoutException">Thrown when the request to Bonus microservice times out.</exception>
    /// <exception cref="PrivilegeHistoryGatewayInternalServerException">Thrown when Bonus microservice returns an internal server error.</exception>
    Task DeleteAsync(int id);
}
