using core.domain;
using core.enums;
using dataaccess.models.postgres;

using PrivilegeHistoryDomain = core.domain.PrivilegeHistory;
using PrivilegeHistoryPostgresqlModel = dataaccess.models.postgres.PrivilegeHistoryPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between PrivilegeHistory domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
/// </summary>
public static class PrivilegeHistoryPostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of PrivilegeHistory</returns>
    public static PrivilegeHistoryDomain ToDomain(PrivilegeHistoryPostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new PrivilegeHistoryDomain
        {
            Id = model.Id,
            PrivilegeId = model.PrivilegeId,
            TicketUid = model.TicketUid,
            DateTime = model.DateTime,
            BalanceDiff = model.BalanceDiff,
            OperationType = (OperationType)model.OperationType,
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of PrivilegeHistory</returns>
    public static PrivilegeHistoryPostgresqlModel ToModel(PrivilegeHistoryDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new PrivilegeHistoryPostgresqlModel
        {
            Id = domain.Id,
            PrivilegeId = domain.PrivilegeId,
            TicketUid = domain.TicketUid,
            DateTime = domain.DateTime,
            BalanceDiff = domain.BalanceDiff,
            OperationType = (int)domain.OperationType,
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<PrivilegeHistoryDomain> ToDomainList(IEnumerable<PrivilegeHistoryPostgresqlModel> models)
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
    public static List<PrivilegeHistoryPostgresqlModel> ToModelList(IEnumerable<PrivilegeHistoryDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
