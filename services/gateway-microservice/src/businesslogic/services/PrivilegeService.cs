using core.domain;
using core.enums;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Privilege business logic operations
/// Provides high-level operations with validation, business rules, and error handling
/// Uses HTTP Gateway to communicate with Bonus microservice
/// </summary>
public class PrivilegeService : IPrivilegeService
{
    private readonly IPrivilegeGateway _privilegeGateway;
    private readonly IPrivilegeHistoryGateway _privilegeHistoryGateway;
    private readonly ILogger<PrivilegeService> _logger;

    /// <summary>
    /// Initializes a new instance of PrivilegeService
    /// </summary>
    /// <param name="privilegeGateway">The Privilege Gateway for HTTP communication</param>
    /// <param name="privilegeHistoryGateway">The PrivilegeHistory Gateway for transaction logging</param>
    /// <param name="logger">Logger for SAGA tracking</param>
    public PrivilegeService(
        IPrivilegeGateway privilegeGateway,
        IPrivilegeHistoryGateway privilegeHistoryGateway,
        ILogger<PrivilegeService> logger)
    {
        _privilegeGateway = privilegeGateway ?? throw new ArgumentNullException(nameof(privilegeGateway));
        _privilegeHistoryGateway = privilegeHistoryGateway ?? throw new ArgumentNullException(nameof(privilegeHistoryGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var privilege = await _privilegeGateway.GetByIdAsync(id);
            if (privilege == null)
            {
                throw new PrivilegeNotFoundException(id);
            }
            return privilege;
        }
        catch (PrivilegeNotFoundException)
        {
            throw;
        }
        catch (PrivilegeGatewayEntityNotFoundException)
        {
            throw new PrivilegeNotFoundException(id);
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for GetByIdAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Privilege with ID {Id}", id);
            throw new ValidationException($"Failed to get Privilege with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Privilege>> GetAllAsync(PrivilegeFilter? filter = null)
    {
        try
        {
            var privileges = filter != null 
                ? await _privilegeGateway.GetAllAsync(filter)
                : await _privilegeGateway.GetAllAsync();
            return privileges.ToList();
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for GetAllAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all Privileges");
            throw new ValidationException("Failed to get all Privileges", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Privilege> CreateAsync(Privilege privilege)
    {
        ValidatePrivilege(privilege);

        try
        {
            var createdPrivilege = await _privilegeGateway.CreateAsync(privilege);
            _logger.LogInformation("Privilege created with ID {Id}", createdPrivilege.Id);
            return createdPrivilege;
        }
        catch (PrivilegeValidationException)
        {
            _logger.LogWarning("Privilege validation failed");
            throw;
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to create Privilege");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Privilege");
            throw new ValidationException("Failed to create Privilege", ex);
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

        try
        {
            var updatedPrivilege = await _privilegeGateway.UpdateAsync(privilege);
            _logger.LogInformation("Privilege {Id} updated successfully", privilege.Id);
            return updatedPrivilege;
        }
        catch (PrivilegeGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Privilege {Id} not found", privilege.Id);
            throw new PrivilegeNotFoundException(privilege.Id);
        }
        catch (PrivilegeValidationException)
        {
            _logger.LogWarning("Privilege validation failed");
            throw;
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to update Privilege {Id}", privilege.Id);
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Privilege {Id}", privilege.Id);
            throw new ValidationException($"Failed to update Privilege with ID {privilege.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeValidationException($"Invalid Privilege ID: {id}. ID must be positive.");
        }

        try
        {
            await _privilegeGateway.DeleteAsync(id);
            _logger.LogInformation("Privilege {Id} deleted successfully", id);
        }
        catch (PrivilegeNotFoundException)
        {
            throw;
        }
        catch (PrivilegeGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Privilege {Id} not found", id);
            throw new PrivilegeNotFoundException(id);
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to delete Privilege {Id}", id);
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Privilege {Id}", id);
            throw new ValidationException($"Failed to delete Privilege with ID {id}", ex);
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
            var privilege = await _privilegeGateway.GetByIdAsync(id);
            return privilege != null;
        }
        catch (PrivilegeGatewayEntityNotFoundException)
        {
            return false;
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for ExistsAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of Privilege with ID {Id}", id);
            throw new ValidationException($"Failed to check existence of Privilege with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(PrivilegeFilter? filter = null)
    {
        try
        {
            var privileges = filter != null 
                ? await _privilegeGateway.GetAllAsync(filter)
                : await _privilegeGateway.GetAllAsync();
            return privileges.Count();
        }
        catch (PrivilegeGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for GetCountAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Privilege count");
            throw new ValidationException("Failed to get Privilege count", ex);
        }
    }

    /// <inheritdoc/>
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

        // Track state for rollback
        Privilege? originalPrivilege = null;
        int? historyId = null;

        try
        {
            // Step 1: Get original privilege (for rollback)
            originalPrivilege = await _privilegeGateway.GetByIdAsync(privilegeId);
            if (originalPrivilege == null)
            {
                throw new PrivilegeNotFoundException(privilegeId);
            }

            // Step 2: Update balance
            originalPrivilege.Balance += amount;
            var updatedPrivilege = await _privilegeGateway.UpdateAsync(originalPrivilege);
            _logger.LogInformation("Privilege {Id} balance credited by {Amount}, new balance: {NewBalance}", 
                privilegeId, amount, updatedPrivilege.Balance);

            // Step 3: Create history record
            var history = new PrivilegeHistory
            {
                PrivilegeId = privilegeId,
                TicketUid = ticketUid,
                DateTime = DateTime.UtcNow,
                BalanceDiff = amount,
                OperationType = OperationType.FILL_IN_BALANCE
            };

            var createdHistory = await _privilegeHistoryGateway.CreateAsync(history);
            historyId = createdHistory.Id;
            _logger.LogInformation("PrivilegeHistory {HistoryId} created for credit operation", historyId);

            return updatedPrivilege;
        }
        catch (PrivilegeValidationException)
        {
            _logger.LogWarning("Validation failed");
            throw;
        }
        catch (PrivilegeGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Privilege {Id} not found", privilegeId);
            throw new PrivilegeNotFoundException(privilegeId);
        }
        catch (Exception ex)
        {
            // Check if it's a communication error before rollback
            if (ex is PrivilegeGatewayCommunicationException || ex is PrivilegeHistoryGatewayCommunicationException)
            {
                _logger.LogError(ex, "CreditBalance communication failed for Privilege {Id}", privilegeId);
                throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
            }

            _logger.LogError(ex, "CreditBalance failed, executing rollback for Privilege {Id}", privilegeId);

            // Rollback in reverse order
            try
            {
                // Rollback Step 1: Delete history record if created
                if (historyId.HasValue)
                {
                    try
                    {
                        await _privilegeHistoryGateway.DeleteAsync(historyId.Value);
                        _logger.LogInformation("Rollback - PrivilegeHistory {HistoryId} deleted", historyId);
                    }
                    catch (Exception historyRollbackEx)
                    {
                        _logger.LogWarning(historyRollbackEx, "Failed to delete history during rollback (may already be deleted)");
                    }
                }

                // Rollback Step 2: Restore original balance
                if (originalPrivilege != null)
                {
                    originalPrivilege.Balance -= amount;
                    await _privilegeGateway.UpdateAsync(originalPrivilege);
                    _logger.LogInformation("Rollback - Privilege {Id} balance restored to {OldBalance}", 
                        privilegeId, originalPrivilege.Balance);
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Rollback failed for Privilege {Id}", privilegeId);
                throw new ValidationException(
                    $"CreditBalance failed and rollback also failed: {rollbackEx.Message}", 
                    rollbackEx);
            }

            throw new ValidationException($"Failed to credit balance for Privilege {privilegeId}", ex);
        }
    }

    /// <inheritdoc/>
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

        // Track state for rollback
        Privilege? originalPrivilege = null;
        int? historyId = null;

        try
        {
            // Step 1: Get original privilege (for rollback)
            originalPrivilege = await _privilegeGateway.GetByIdAsync(privilegeId);
            if (originalPrivilege == null)
            {
                throw new PrivilegeNotFoundException(privilegeId);
            }

            // Step 1.5: Check sufficient balance
            if (originalPrivilege.Balance < amount)
            {
                throw new PrivilegeBusinessRuleViolationException(
                    "InsufficientBalance",
                    $"Insufficient balance. Current: {originalPrivilege.Balance}, Requested: {amount}");
            }

            // Step 2: Update balance
            originalPrivilege.Balance -= amount;
            var updatedPrivilege = await _privilegeGateway.UpdateAsync(originalPrivilege);
            _logger.LogInformation("Privilege {Id} balance debited by {Amount}, new balance: {NewBalance}", 
                privilegeId, amount, updatedPrivilege.Balance);

            // Step 3: Create history record
            var history = new PrivilegeHistory
            {
                PrivilegeId = privilegeId,
                TicketUid = ticketUid,
                DateTime = DateTime.UtcNow,
                BalanceDiff = -amount,
                OperationType = OperationType.DEBIT_THE_ACCOUNT
            };

            var createdHistory = await _privilegeHistoryGateway.CreateAsync(history);
            historyId = createdHistory.Id;
            _logger.LogInformation("PrivilegeHistory {HistoryId} created for debit operation", historyId);

            return updatedPrivilege;
        }
        catch (PrivilegeValidationException)
        {
            _logger.LogWarning("Validation failed");
            throw;
        }
        catch (PrivilegeBusinessRuleViolationException)
        {
            _logger.LogWarning("Business rule violation (insufficient balance)");
            throw;
        }
        catch (PrivilegeGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Privilege {Id} not found", privilegeId);
            throw new PrivilegeNotFoundException(privilegeId);
        }
        catch (Exception ex)
        {
            // Check if it's a communication error before rollback
            if (ex is PrivilegeGatewayCommunicationException || ex is PrivilegeHistoryGatewayCommunicationException)
            {
                _logger.LogError(ex, "DebitBalance communication failed for Privilege {Id}", privilegeId);
                throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
            }

            _logger.LogError(ex, "DebitBalance failed, executing rollback for Privilege {Id}", privilegeId);

            // Rollback in reverse order
            try
            {
                // Rollback Step 1: Delete history record if created
                if (historyId.HasValue)
                {
                    try
                    {
                        await _privilegeHistoryGateway.DeleteAsync(historyId.Value);
                        _logger.LogInformation("Rollback - PrivilegeHistory {HistoryId} deleted", historyId);
                    }
                    catch (Exception historyRollbackEx)
                    {
                        _logger.LogWarning(historyRollbackEx, "Failed to delete history during rollback (may already be deleted)");
                    }
                }

                // Rollback Step 2: Restore original balance
                if (originalPrivilege != null)
                {
                    originalPrivilege.Balance += amount;
                    await _privilegeGateway.UpdateAsync(originalPrivilege);
                    _logger.LogInformation("Rollback - Privilege {Id} balance restored to {OldBalance}", 
                        privilegeId, originalPrivilege.Balance);
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Rollback failed for Privilege {Id}", privilegeId);
                throw new ValidationException(
                    $"DebitBalance failed and rollback also failed: {rollbackEx.Message}", 
                    rollbackEx);
            }

            throw new ValidationException($"Failed to debit balance for Privilege {privilegeId}", ex);
        }
    }

    /// <inheritdoc/>
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
