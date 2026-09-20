using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;
using core.exceptions.dataaccess.gateways;

namespace businesslogic.services;

/// <summary>
/// Service implementation for PrivilegeHistory business logic operations
/// Provides high-level operations with validation and business rules
/// Uses HTTP Gateway to communicate with Bonus microservice
/// </summary>
public class PrivilegeHistoryService : IPrivilegeHistoryService
{
    private readonly IPrivilegeHistoryGateway _privilegeHistoryGateway;
    private readonly ILogger<PrivilegeHistoryService> _logger;

    /// <summary>
    /// Initializes a new instance of PrivilegeHistoryService
    /// </summary>
    /// <param name="privilegeHistoryGateway">The PrivilegeHistory Gateway for HTTP communication</param>
    /// <param name="logger">Logger for tracking operations</param>
    public PrivilegeHistoryService(IPrivilegeHistoryGateway privilegeHistoryGateway, ILogger<PrivilegeHistoryService> logger)
    {
        _privilegeHistoryGateway = privilegeHistoryGateway ?? throw new ArgumentNullException(nameof(privilegeHistoryGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<PrivilegeHistory> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeHistoryValidationException($"Invalid PrivilegeHistory ID: {id}. ID must be positive.");
        }

        try
        {
            var history = await _privilegeHistoryGateway.GetByIdAsync(id);
            if (history == null)
            {
                throw new PrivilegeHistoryNotFoundException(id);
            }
            return history;
        }
        catch (PrivilegeHistoryNotFoundException)
        {
            throw;
        }
        catch (PrivilegeHistoryGatewayEntityNotFoundException)
        {
            throw new PrivilegeHistoryNotFoundException(id);
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for GetByIdAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PrivilegeHistory with ID {Id}", id);
            throw new ValidationException($"Failed to get PrivilegeHistory with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<PrivilegeHistory>> GetAllAsync(PrivilegeHistoryFilter? filter = null)
    {
        try
        {
            var histories = filter != null 
                ? await _privilegeHistoryGateway.GetAllAsync(filter)
                : await _privilegeHistoryGateway.GetAllAsync();
            return histories.ToList();
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for GetAllAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all PrivilegeHistory records");
            throw new ValidationException("Failed to get all PrivilegeHistory records", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<PrivilegeHistory> CreateAsync(PrivilegeHistory history)
    {
        ValidatePrivilegeHistory(history);

        try
        {
            var createdHistory = await _privilegeHistoryGateway.CreateAsync(history);
            _logger.LogInformation("PrivilegeHistory created with ID {Id}", createdHistory.Id);
            return createdHistory;
        }
        catch (PrivilegeHistoryValidationException)
        {
            _logger.LogWarning("PrivilegeHistory validation failed");
            throw;
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to create PrivilegeHistory");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PrivilegeHistory");
            throw new ValidationException("Failed to create PrivilegeHistory", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<PrivilegeHistory> UpdateAsync(PrivilegeHistory history)
    {
        if (history.Id <= 0)
        {
            throw new PrivilegeHistoryValidationException($"Invalid PrivilegeHistory ID: {history.Id}. ID must be positive.");
        }

        ValidatePrivilegeHistory(history);

        // Check if history exists before updating
        try
        {
            var exists = await _privilegeHistoryGateway.GetByIdAsync(history.Id);
            if (exists == null)
            {
                throw new PrivilegeHistoryNotFoundException(history.Id);
            }
        }
        catch (PrivilegeHistoryGatewayEntityNotFoundException)
        {
            throw new PrivilegeHistoryNotFoundException(history.Id);
        }

        try
        {
            var updatedHistory = await _privilegeHistoryGateway.UpdateAsync(history);
            _logger.LogInformation("PrivilegeHistory {Id} updated successfully", history.Id);
            return updatedHistory;
        }
        catch (PrivilegeHistoryGatewayEntityNotFoundException)
        {
            _logger.LogWarning("PrivilegeHistory {Id} not found", history.Id);
            throw new PrivilegeHistoryNotFoundException(history.Id);
        }
        catch (PrivilegeHistoryValidationException)
        {
            _logger.LogWarning("PrivilegeHistory validation failed");
            throw;
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to update PrivilegeHistory {Id}", history.Id);
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update PrivilegeHistory {Id}", history.Id);
            throw new ValidationException($"Failed to update PrivilegeHistory with ID {history.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeHistoryValidationException($"Invalid PrivilegeHistory ID: {id}. ID must be positive.");
        }

        // Check if history exists before deleting
        try
        {
            var exists = await _privilegeHistoryGateway.GetByIdAsync(id);
            if (exists == null)
            {
                throw new PrivilegeHistoryNotFoundException(id);
            }
        }
        catch (PrivilegeHistoryGatewayEntityNotFoundException)
        {
            throw new PrivilegeHistoryNotFoundException(id);
        }

        try
        {
            await _privilegeHistoryGateway.DeleteAsync(id);
            _logger.LogInformation("PrivilegeHistory {Id} deleted successfully", id);
        }
        catch (PrivilegeHistoryNotFoundException)
        {
            throw;
        }
        catch (PrivilegeHistoryGatewayEntityNotFoundException)
        {
            _logger.LogWarning("PrivilegeHistory {Id} not found", id);
            throw new PrivilegeHistoryNotFoundException(id);
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to delete PrivilegeHistory {Id}", id);
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete PrivilegeHistory {Id}", id);
            throw new ValidationException($"Failed to delete PrivilegeHistory with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeHistoryValidationException($"Invalid PrivilegeHistory ID: {id}. ID must be positive.");
        }

        try
        {
            var history = await _privilegeHistoryGateway.GetByIdAsync(id);
            return history != null;
        }
        catch (PrivilegeHistoryGatewayEntityNotFoundException)
        {
            return false;
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for ExistsAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of PrivilegeHistory with ID {Id}", id);
            throw new ValidationException($"Failed to check existence of PrivilegeHistory with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(PrivilegeHistoryFilter? filter = null)
    {
        try
        {
            var histories = filter != null 
                ? await _privilegeHistoryGateway.GetAllAsync(filter)
                : await _privilegeHistoryGateway.GetAllAsync();
            return histories.Count();
        }
        catch (PrivilegeHistoryGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Bonus microservice for GetCountAsync");
            throw new ValidationException($"Failed to communicate with Bonus microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PrivilegeHistory count");
            throw new ValidationException("Failed to get PrivilegeHistory count", ex);
        }
    }

    /// <summary>
    /// Validates PrivilegeHistory entity for business rules
    /// </summary>
    /// <param name="history">The PrivilegeHistory to validate</param>
    /// <exception cref="PrivilegeHistoryValidationException">Thrown when validation fails</exception>
    private void ValidatePrivilegeHistory(PrivilegeHistory history)
    {
        if (history == null)
        {
            throw new PrivilegeHistoryValidationException("PrivilegeHistory cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate PrivilegeId
        if (history.PrivilegeId <= 0)
        {
            errors["PrivilegeId"] = new[] { "PrivilegeId must be positive" };
        }

        // Validate BalanceDiff (cannot be zero)
        if (history.BalanceDiff == 0)
        {
            errors["BalanceDiff"] = new[] { "BalanceDiff cannot be zero" };
        }

        // Validate DateTime (must be in the past or present)
        if (history.DateTime > DateTime.UtcNow)
        {
            errors["DateTime"] = new[] { "DateTime cannot be in the future" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"PrivilegeHistory validation failed with {errors.Count} error(s)";
            throw new PrivilegeHistoryValidationException(errorMessage, errors);
        }
    }
}
