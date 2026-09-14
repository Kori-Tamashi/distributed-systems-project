using core.domain;
using core.enums;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryPrivilegeNotFoundException = core.exceptions.dataaccess.repositories.PrivilegeNotFoundException;
using RepositoryPrivilegeAlreadyExistsException = core.exceptions.dataaccess.repositories.PrivilegeAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Privilege business logic operations
/// Provides high-level operations with validation and business rules
/// </summary>
public class PrivilegeService : IPrivilegeService
{
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly IPrivilegeHistoryRepository _privilegeHistoryRepository;

    /// <summary>
    /// Initializes a new instance of PrivilegeService
    /// </summary>
    /// <param name="privilegeRepository">The Privilege repository for data access</param>
    /// <param name="privilegeHistoryRepository">The PrivilegeHistory repository for transaction logging</param>
    public PrivilegeService(
        IPrivilegeRepository privilegeRepository,
        IPrivilegeHistoryRepository privilegeHistoryRepository)
    {
        _privilegeRepository = privilegeRepository ?? throw new ArgumentNullException(nameof(privilegeRepository));
        _privilegeHistoryRepository = privilegeHistoryRepository ?? throw new ArgumentNullException(nameof(privilegeHistoryRepository));
    }

    /// <inheritdoc/>
    public async Task<Privilege> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {id}. ID must be positive.");
        }

        try
        {
            var privilege = await _privilegeRepository.GetByIdAsync(id);
            return privilege ?? throw new PrivilegeNotFoundException(id);
        }
        catch (RepositoryPrivilegeNotFoundException)
        {
            throw new PrivilegeNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get Privilege with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Privilege>> GetAllAsync(PrivilegeFilter? filter = null)
    {
        try
        {
            return await _privilegeRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all Privileges", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Privilege> CreateAsync(Privilege privilege)
    {
        ValidatePrivilege(privilege);

        try
        {
            var createdPrivilege = await _privilegeRepository.CreateAsync(privilege);
            return createdPrivilege;
        }
        catch (PrivilegeValidationException)
        {
            throw;
        }
        catch (RepositoryPrivilegeAlreadyExistsException)
        {
            throw new PrivilegeBusinessRuleViolationException(
                "UniqueConstraint", 
                $"Privilege with username {privilege.Username} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create Privilege", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Privilege> UpdateAsync(Privilege privilege)
    {
        if (privilege.Id <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {privilege.Id}. ID must be positive.");
        }

        ValidatePrivilege(privilege);

        // Check if privilege exists before updating (outside try-catch)
        var exists = await _privilegeRepository.ExistsAsync(privilege.Id);
        if (!exists)
        {
            throw new PrivilegeNotFoundException(privilege.Id);
        }

        try
        {
            var updatedPrivilege = await _privilegeRepository.UpdateAsync(privilege);
            return updatedPrivilege;
        }
        catch (RepositoryPrivilegeNotFoundException)
        {
            throw new PrivilegeNotFoundException(privilege.Id);
        }
        catch (PrivilegeValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update Privilege with ID {privilege.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {id}. ID must be positive.");
        }

        // Check if privilege exists before deleting (outside try-catch)
        var exists = await _privilegeRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new PrivilegeNotFoundException(id);
        }

        try
        {
            return await _privilegeRepository.DeleteAsync(id);
        }
        catch (RepositoryPrivilegeNotFoundException)
        {
            throw new PrivilegeNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete Privilege with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {id}. ID must be positive.");
        }

        try
        {
            return await _privilegeRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of Privilege with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(PrivilegeFilter? filter = null)
    {
        try
        {
            return await _privilegeRepository.GetCountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get Privilege count", ex);
        }
    }

    /// <summary>
    /// Credits balance to a privilege
    /// </summary>
    /// <param name="privilegeId">The privilege ID</param>
    /// <param name="amount">Amount to credit (positive)</param>
    /// <param name="ticketUid">Ticket UID associated with the operation</param>
    /// <returns>The updated privilege</returns>
    public async Task<Privilege> CreditBalanceAsync(int privilegeId, int amount, Guid ticketUid)
    {
        if (privilegeId <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {privilegeId}. ID must be positive.");
        }

        if (amount <= 0)
        {
            throw new PrivilegeValidationException($"Invalid amount: {amount}. Amount must be positive.");
        }

        var privilege = await GetByIdAsync(privilegeId);

        privilege.Balance += amount;

        // Create history record
        var history = new PrivilegeHistory
        {
            PrivilegeId = privilegeId,
            TicketUid = ticketUid,
            DateTime = DateTime.UtcNow,
            BalanceDiff = amount,
            OperationType = OperationType.FILL_IN_BALANCE
        };

        await _privilegeHistoryRepository.CreateAsync(history);

        return await UpdateAsync(privilege);
    }

    /// <summary>
    /// Debits balance from a privilege
    /// </summary>
    /// <param name="privilegeId">The privilege ID</param>
    /// <param name="amount">Amount to debit (positive)</param>
    /// <param name="ticketUid">Ticket UID associated with the operation</param>
    /// <returns>The updated privilege</returns>
    public async Task<Privilege> DebitBalanceAsync(int privilegeId, int amount, Guid ticketUid)
    {
        if (privilegeId <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {privilegeId}. ID must be positive.");
        }

        if (amount <= 0)
        {
            throw new PrivilegeValidationException($"Invalid amount: {amount}. Amount must be positive.");
        }

        var privilege = await GetByIdAsync(privilegeId);

        if (privilege.Balance < amount)
        {
            throw new PrivilegeBusinessRuleViolationException(
                "InsufficientBalance",
                $"Insufficient balance. Current: {privilege.Balance}, Requested: {amount}");
        }

        privilege.Balance -= amount;

        // Create history record
        var history = new PrivilegeHistory
        {
            PrivilegeId = privilegeId,
            TicketUid = ticketUid,
            DateTime = DateTime.UtcNow,
            BalanceDiff = -amount,
            OperationType = OperationType.DEBIT_THE_ACCOUNT
        };

        await _privilegeHistoryRepository.CreateAsync(history);

        return await UpdateAsync(privilege);
    }

    /// <summary>
    /// Gets the maximum amount that can be debited from a privilege
    /// </summary>
    /// <param name="privilegeId">The privilege ID</param>
    /// <returns>The maximum debit amount</returns>
    public async Task<int> GetMaxDebitAmountAsync(int privilegeId)
    {
        var privilege = await GetByIdAsync(privilegeId);
        return privilege.Balance;
    }

    /// <summary>
    /// Validates Privilege entity for business rules
    /// </summary>
    /// <param name="privilege">The Privilege to validate</param>
    /// <exception cref="PrivilegeValidationException">Thrown when validation fails</exception>
    private void ValidatePrivilege(Privilege privilege)
    {
        if (privilege == null)
        {
            throw new PrivilegeValidationException("Privilege cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate Username
        if (string.IsNullOrWhiteSpace(privilege.Username))
        {
            errors["Username"] = new[] { "Username is required and cannot be empty" };
        }
        else if (privilege.Username.Length > 80)
        {
            errors["Username"] = new[] { "Username cannot exceed 80 characters" };
        }

        // Validate Balance (must be non-negative)
        if (privilege.Balance < 0)
        {
            errors["Balance"] = new[] { "Balance cannot be negative" };
        }
        else if (privilege.Balance > int.MaxValue)
        {
            errors["Balance"] = new[] { "Balance exceeds maximum allowed value" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Privilege validation failed with {errors.Count} error(s)";
            throw new PrivilegeValidationException(errorMessage, errors);
        }
    }
}
