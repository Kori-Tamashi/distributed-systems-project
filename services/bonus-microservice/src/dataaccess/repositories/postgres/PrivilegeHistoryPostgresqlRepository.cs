using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using PrivilegeHistoryDomain = core.domain.PrivilegeHistory;
using PrivilegeHistoryFilter = core.filters.PrivilegeHistoryFilter;
using PrivilegeHistoryPostgresqlModel = dataaccess.models.postgres.PrivilegeHistoryPostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of IPrivilegeHistoryRepository
/// Provides data access operations for PrivilegeHistory entities
/// </summary>
public class PrivilegeHistoryPostgresqlRepository : IPrivilegeHistoryRepository
{
    private readonly BonusDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public PrivilegeHistoryPostgresqlRepository(BonusDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a PrivilegeHistory by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the PrivilegeHistory</param>
    /// <returns>The PrivilegeHistory entity if found</returns>
    /// <exception cref="PrivilegeHistoryNotFoundException">Thrown when PrivilegeHistory with specified id is not found</exception>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<PrivilegeHistoryDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.PrivilegeHistories.FindAsync(id)
                ?? throw new PrivilegeHistoryNotFoundException(id);

            return PrivilegeHistoryPostgresqlConverter.ToDomain(model);
        }
        catch (PrivilegeHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException($"Failed to get PrivilegeHistory by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all PrivilegeHistory entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying history (null returns all)</param>
    /// <returns>List of all PrivilegeHistory entities matching the filter or all history if filter is null</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<PrivilegeHistoryDomain>> GetAllAsync(PrivilegeHistoryFilter? filter = null)
    {
        try
        {
            var query = _context.PrivilegeHistories.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return PrivilegeHistoryPostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException("Failed to get all PrivilegeHistories", ex);
        }
    }

    /// <summary>
    /// Creates a new PrivilegeHistory
    /// </summary>
    /// <param name="history">The PrivilegeHistory entity to create</param>
    /// <returns>The created PrivilegeHistory entity with generated Id</returns>
    /// <exception cref="PrivilegeHistoryAlreadyExistsException">Thrown when PrivilegeHistory with same id already exists</exception>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<PrivilegeHistoryDomain> CreateAsync(PrivilegeHistoryDomain history)
    {
        try
        {
            // Check if PrivilegeHistory already exists
            if (await ExistsAsync(history.Id))
            {
                throw new PrivilegeHistoryAlreadyExistsException(history.Id);
            }

            var model = PrivilegeHistoryPostgresqlConverter.ToModel(history);
            _context.PrivilegeHistories.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
        }
        catch (PrivilegeHistoryAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException("Failed to create PrivilegeHistory", ex);
        }
    }

    /// <summary>
    /// Updates an existing PrivilegeHistory
    /// </summary>
    /// <param name="history">The PrivilegeHistory entity to update</param>
    /// <returns>The updated PrivilegeHistory entity</returns>
    /// <exception cref="PrivilegeHistoryNotFoundException">Thrown when PrivilegeHistory with specified id is not found</exception>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<PrivilegeHistoryDomain> UpdateAsync(PrivilegeHistoryDomain history)
    {
        try
        {
            var existingModel = await _context.PrivilegeHistories.FindAsync(history.Id)
                ?? throw new PrivilegeHistoryNotFoundException(history.Id);

            // Update properties
            existingModel.PrivilegeId = history.PrivilegeId;
            existingModel.TicketUid = history.TicketUid;
            existingModel.DateTime = history.DateTime;
            existingModel.BalanceDiff = history.BalanceDiff;
            existingModel.OperationType = (int)history.OperationType;

            await _context.SaveChangesAsync();

            return PrivilegeHistoryPostgresqlConverter.ToDomain(existingModel);
        }
        catch (PrivilegeHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException($"Failed to update PrivilegeHistory with id {history.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a PrivilegeHistory by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the PrivilegeHistory to delete</param>
    /// <returns>True if PrivilegeHistory was deleted, false if not found</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.PrivilegeHistories.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.PrivilegeHistories.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException($"Failed to delete PrivilegeHistory with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if a PrivilegeHistory exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if PrivilegeHistory exists, false otherwise</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.PrivilegeHistories.AnyAsync(h => h.Id == id);
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException($"Failed to check if PrivilegeHistory with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of PrivilegeHistory entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting history (null counts all)</param>
    /// <returns>Total number of PrivilegeHistory entities matching the filter</returns>
    /// <exception cref="PrivilegeHistoryDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync(PrivilegeHistoryFilter? filter = null)
    {
        try
        {
            var query = _context.PrivilegeHistories.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new PrivilegeHistoryDatabaseException("Failed to get PrivilegeHistories count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<PrivilegeHistoryPostgresqlModel> ApplyFilter(
        IQueryable<PrivilegeHistoryPostgresqlModel> query,
        PrivilegeHistoryFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by PrivilegeId
        if (filter.PrivilegeId.HasValue)
        {
            query = query.Where(h => h.PrivilegeId == filter.PrivilegeId.Value);
        }

        // Filter by TicketUid
        if (filter.TicketUid.HasValue)
        {
            query = query.Where(h => h.TicketUid == filter.TicketUid.Value);
        }

        // Filter by OperationType
        if (filter.OperationType.HasValue)
        {
            query = query.Where(h => h.OperationType == (int)filter.OperationType.Value);
        }

        // Filter by MinBalanceDiff
        if (filter.MinBalanceDiff.HasValue)
        {
            query = query.Where(h => h.BalanceDiff >= filter.MinBalanceDiff.Value);
        }

        // Filter by MaxBalanceDiff
        if (filter.MaxBalanceDiff.HasValue)
        {
            query = query.Where(h => h.BalanceDiff <= filter.MaxBalanceDiff.Value);
        }

        // Filter by DateTimeFrom
        if (filter.DateTimeFrom.HasValue)
        {
            query = query.Where(h => h.DateTime >= filter.DateTimeFrom.Value);
        }

        // Filter by DateTimeUntil
        if (filter.DateTimeUntil.HasValue)
        {
            query = query.Where(h => h.DateTime <= filter.DateTimeUntil.Value);
        }

        return query;
    }
}
