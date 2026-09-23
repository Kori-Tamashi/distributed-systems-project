using core.domain;
using core.enums;
using dataaccess.models.postgres;

using PrivilegeDomain = core.domain.Privilege;
using PrivilegePostgresqlModel = dataaccess.models.postgres.PrivilegePostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Privilege domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
/// </summary>
public static class PrivilegePostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of Privilege</returns>
    public static PrivilegeDomain ToDomain(PrivilegePostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new PrivilegeDomain
        {
            Id = model.Id,
            Username = model.Username,
            Status = (PrivilegeStatus)model.Status,
            Balance = model.Balance,
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of Privilege</returns>
    public static PrivilegePostgresqlModel ToModel(PrivilegeDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new PrivilegePostgresqlModel
        {
            Id = domain.Id,
            Username = domain.Username,
            Status = (int)domain.Status,
            Balance = domain.Balance,
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<PrivilegeDomain> ToDomainList(IEnumerable<PrivilegePostgresqlModel> models)
    {
        if (models == null)
            throw new ArgumentNullException(nameof(models));

        return models.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts a collection of domain entities to PostgreSQL models
    /// </summary>
    /// <param name="domains">Collection of domain entities</param>
    /// <returns>Collection of PostgreSQL models</returns>
    public static List<PrivilegePostgresqlModel> ToModelList(IEnumerable<PrivilegeDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
