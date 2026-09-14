using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryPrivilegeHistoryNotFoundException = core.exceptions.dataaccess.repositories.PrivilegeHistoryNotFoundException;
using RepositoryPrivilegeHistoryAlreadyExistsException = core.exceptions.dataaccess.repositories.PrivilegeHistoryAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for PrivilegeHistory business logic operations
/// Provides high-level operations with validation and business rules
/// </summary>
public class PrivilegeHistoryService : IPrivilegeHistoryService
{
    private readonly IPrivilegeHistoryRepository _privilegeHistoryRepository;

    /// <summary>
    /// Initializes a new instance of PrivilegeHistoryService
    /// </summary>
    /// <param name="privilegeHistoryRepository">The PrivilegeHistory repository for data access</param>
    public PrivilegeHistoryService(IPrivilegeHistoryRepository privilegeHistoryRepository)
    {
        _privilegeHistoryRepository = privilegeHistoryRepository ?? throw new ArgumentNullException(nameof(privilegeHistoryRepository));
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
            var history = await _privilegeHistoryRepository.GetByIdAsync(id);
            return history ?? throw new PrivilegeHistoryNotFoundException(id);
        }
        catch (RepositoryPrivilegeHistoryNotFoundException)
        {
            throw new PrivilegeHistoryNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get PrivilegeHistory with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<PrivilegeHistory>> GetAllAsync(PrivilegeHistoryFilter? filter = null)
    {
        try
        {
            return await _privilegeHistoryRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all PrivilegeHistory records", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<PrivilegeHistory> CreateAsync(PrivilegeHistory history)
    {
        ValidatePrivilegeHistory(history);

        try
        {
            var createdHistory = await _privilegeHistoryRepository.CreateAsync(history);
            return createdHistory;
        }
        catch (PrivilegeHistoryValidationException)
        {
            throw;
        }
        catch (RepositoryPrivilegeHistoryAlreadyExistsException)
        {
            throw new PrivilegeHistoryBusinessRuleViolationException(
                "UniqueConstraint", 
                $"PrivilegeHistory with ID {history.Id} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create PrivilegeHistory", ex);
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

        // Check if history exists before updating (outside try-catch)
        var exists = await _privilegeHistoryRepository.ExistsAsync(history.Id);
        if (!exists)
        {
            throw new PrivilegeHistoryNotFoundException(history.Id);
        }

        try
        {
            var updatedHistory = await _privilegeHistoryRepository.UpdateAsync(history);
            return updatedHistory;
        }
        catch (RepositoryPrivilegeHistoryNotFoundException)
        {
            throw new PrivilegeHistoryNotFoundException(history.Id);
        }
        catch (PrivilegeHistoryValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update PrivilegeHistory with ID {history.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new PrivilegeHistoryValidationException($"Invalid PrivilegeHistory ID: {id}. ID must be positive.");
        }

        // Check if history exists before deleting (outside try-catch)
        var exists = await _privilegeHistoryRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new PrivilegeHistoryNotFoundException(id);
        }

        try
        {
            return await _privilegeHistoryRepository.DeleteAsync(id);
        }
        catch (RepositoryPrivilegeHistoryNotFoundException)
        {
            throw new PrivilegeHistoryNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete PrivilegeHistory with ID {id}", ex);
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
            return await _privilegeHistoryRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of PrivilegeHistory with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(PrivilegeHistoryFilter? filter = null)
    {
        try
        {
            return await _privilegeHistoryRepository.GetCountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get PrivilegeHistory count", ex);
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
