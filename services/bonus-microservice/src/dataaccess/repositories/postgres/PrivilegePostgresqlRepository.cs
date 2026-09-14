using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using PrivilegeDomain = core.domain.Privilege;
using PrivilegeFilter = core.filters.PrivilegeFilter;
using PrivilegePostgresqlModel = dataaccess.models.postgres.PrivilegePostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of IPrivilegeRepository
/// Provides data access operations for Privilege entities
/// </summary>
public class PrivilegePostgresqlRepository : IPrivilegeRepository
{
    private readonly BonusDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public PrivilegePostgresqlRepository(BonusDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a Privilege by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Privilege</param>
    /// <returns>The Privilege entity if found</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<PrivilegeDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.Privileges.FindAsync(id)
                ?? throw new PrivilegeNotFoundException(id);

            return PrivilegePostgresqlConverter.ToDomain(model);
        }
        catch (PrivilegeNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException($"Failed to get Privilege by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all Privilege entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying privileges (null returns all)</param>
    /// <returns>List of Privilege entities matching the filter or all privileges if filter is null</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<PrivilegeDomain>> GetAllAsync(PrivilegeFilter? filter = null)
    {
        try
        {
            var query = _context.Privileges.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return PrivilegePostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException("Failed to get all Privileges", ex);
        }
    }

    /// <summary>
    /// Creates a new Privilege
    /// </summary>
    /// <param name="privilege">The Privilege entity to create</param>
    /// <returns>The created Privilege entity with generated Id</returns>
    /// <exception cref="PrivilegeAlreadyExistsException">Thrown when Privilege with same id already exists</exception>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<PrivilegeDomain> CreateAsync(PrivilegeDomain privilege)
    {
        try
        {
            // Check if Privilege already exists
            if (await ExistsAsync(privilege.Id))
            {
                throw new PrivilegeAlreadyExistsException(privilege.Id);
            }

            var model = PrivilegePostgresqlConverter.ToModel(privilege);
            _context.Privileges.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
        }
        catch (PrivilegeAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException("Failed to create Privilege", ex);
        }
    }

    /// <summary>
    /// Updates an existing Privilege
    /// </summary>
    /// <param name="privilege">The Privilege entity to update</param>
    /// <returns>The updated Privilege entity</returns>
    /// <exception cref="PrivilegeNotFoundException">Thrown when Privilege with specified id is not found</exception>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<PrivilegeDomain> UpdateAsync(PrivilegeDomain privilege)
    {
        try
        {
            var existingModel = await _context.Privileges.FindAsync(privilege.Id)
                ?? throw new PrivilegeNotFoundException(privilege.Id);

            // Update properties
            existingModel.Username = privilege.Username;
            existingModel.Status = (int)privilege.Status;
            existingModel.Balance = privilege.Balance;

            await _context.SaveChangesAsync();

            return PrivilegePostgresqlConverter.ToDomain(existingModel);
        }
        catch (PrivilegeNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException($"Failed to update Privilege with id {privilege.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a Privilege by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Privilege to delete</param>
    /// <returns>True if Privilege was deleted, false if not found</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.Privileges.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Privileges.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException($"Failed to delete Privilege with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if a Privilege exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Privilege exists, false otherwise</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.Privileges.AnyAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException($"Failed to check if Privilege with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of Privilege entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting privileges (null counts all)</param>
    /// <returns>Total number of Privilege entities matching the filter</returns>
    /// <exception cref="PrivilegeDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync(PrivilegeFilter? filter = null)
    {
        try
        {
            var query = _context.Privileges.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            throw new PrivilegeDatabaseException("Failed to get Privileges count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<PrivilegePostgresqlModel> ApplyFilter(
        IQueryable<PrivilegePostgresqlModel> query,
        PrivilegeFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by Username (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Username))
        {
            var username = filter.Username.ToLower();
            query = query.Where(p => p.Username.ToLower().Contains(username));
        }

        // Filter by Status
        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == (int)filter.Status.Value);
        }

        // Filter by MinBalance
        if (filter.MinBalance.HasValue)
        {
            query = query.Where(p => p.Balance >= filter.MinBalance.Value);
        }

        // Filter by MaxBalance
        if (filter.MaxBalance.HasValue)
        {
            query = query.Where(p => p.Balance <= filter.MaxBalance.Value);
        }

        return query;
    }
}
