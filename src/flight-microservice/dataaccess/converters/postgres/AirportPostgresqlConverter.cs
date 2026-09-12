using core.domain;
using dataaccess.models.postgres;

using AirportDomain = core.domain.Airport;
using AirportPostgresqlModel = dataaccess.models.postgres.AirportPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Airport domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
/// </summary>
public static class AirportPostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of Airport</returns>
    public static AirportDomain ToDomain(AirportPostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new AirportDomain
        {
            Id = model.Id,
            Name = model.Name,
            City = model.City,
            Country = model.Country
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of Airport</returns>
    public static AirportPostgresqlModel ToModel(AirportDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new AirportPostgresqlModel
        {
            Id = domain.Id,
            Name = domain.Name,
            City = domain.City,
            Country = domain.Country
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<AirportDomain> ToDomainList(IEnumerable<AirportPostgresqlModel> models)
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
    public static List<AirportPostgresqlModel> ToModelList(IEnumerable<AirportDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
