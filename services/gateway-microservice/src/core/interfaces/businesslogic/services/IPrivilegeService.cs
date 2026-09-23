using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;

namespace core.interfaces.businesslogic.services;

/// <summary>
/// Service interface for Privilege business logic operations
/// Provides high-level operations for managing Privilege entities
/// </summary>
public interface IPrivilegeService
{
    /// <summary>
    /// Gets a Privilege by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Privilege</param>
    /// <returns>The Privilege entity if found</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Privilege> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Privilege entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying privileges (null returns all)</param>
    /// <returns>List of Privilege entities matching the filter or all privileges if filter is null</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<List<Privilege>> GetAllAsync(PrivilegeFilter? filter = null);

    /// <summary>
    /// Creates a new Privilege
    /// </summary>
    /// <param name="privilege">The Privilege entity to create</param>
    /// <returns>The created Privilege entity with generated Id</returns>
    /// <exception cref="PrivilegeValidationException">Thrown when Privilege data is invalid</exception>
    /// <exception cref="PrivilegeBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Privilege> CreateAsync(Privilege privilege);

    /// <summary>
    /// Updates an existing Privilege
    /// </summary>
    /// <param name="privilege">The Privilege entity to update</param>
    /// <returns>The updated Privilege entity</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="PrivilegeValidationException">Thrown when Privilege data is invalid</exception>
    /// <exception cref="PrivilegeBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Privilege> UpdateAsync(Privilege privilege);

    /// <summary>
    /// Deletes a Privilege by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Privilege to delete</param>
    /// <returns>True if Privilege was deleted successfully</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if a Privilege exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Privilege exists, false otherwise</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Privilege entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting privileges (null counts all)</param>
    /// <returns>Total number of Privilege entities matching the filter</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetCountAsync(PrivilegeFilter? filter = null);

    /// <summary>
    /// Credits balance to a privilege
    /// </summary>
    /// <param name="privilegeId">The privilege ID</param>
    /// <param name="amount">Amount to credit (positive)</param>
    /// <param name="ticketUid">Ticket UID associated with the operation</param>
    /// <returns>The updated privilege</returns>
    /// <exception cref="PrivilegeValidationException">Thrown when parameters are invalid</exception>
    /// <exception cref="PrivilegeNotFoundException">Thrown when privilege is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Privilege> CreditBalanceAsync(int privilegeId, int amount, Guid ticketUid);

    /// <summary>
    /// Debits balance from a privilege
    /// </summary>
    /// <param name="privilegeId">The privilege ID</param>
    /// <param name="amount">Amount to debit (positive)</param>
    /// <param name="ticketUid">Ticket UID associated with the operation</param>
    /// <returns>The updated privilege</returns>
    /// <exception cref="PrivilegeValidationException">Thrown when parameters are invalid</exception>
    /// <exception cref="PrivilegeNotFoundException">Thrown when privilege is not found</exception>
    /// <exception cref="PrivilegeBusinessRuleViolationException">Thrown when balance is insufficient</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Privilege> DebitBalanceAsync(int privilegeId, int amount, Guid ticketUid);

    /// <summary>
    /// Gets the maximum amount that can be debited from a privilege
    /// </summary>
    /// <param name="privilegeId">The privilege ID</param>
    /// <returns>The maximum debit amount (current balance)</returns>
    /// <exception cref="PrivilegeValidationException">Thrown when privilege ID is invalid</exception>
    /// <exception cref="PrivilegeNotFoundException">Thrown when privilege is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetMaxDebitAmountAsync(int privilegeId);
}
