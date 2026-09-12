using core.domain;
using dataaccess.models.postgres;

using FlightDomain = core.domain.Flight;
using FlightPostgresqlModel = dataaccess.models.postgres.FlightPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Flight domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
/// </summary>
public static class FlightPostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of Flight</returns>
    public static FlightDomain ToDomain(FlightPostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new FlightDomain
        {
            Id = model.Id,
            FlightUid = model.FlightUid,
            FlightNumber = model.FlightNumber,
            DateTime = model.DateTime,
            FromAirportId = model.FromAirportId,
            ToAirportId = model.ToAirportId,
            Price = model.Price
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of Flight</returns>
    public static FlightPostgresqlModel ToModel(FlightDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new FlightPostgresqlModel
        {
            Id = domain.Id,
            FlightUid = domain.FlightUid,
            FlightNumber = domain.FlightNumber,
            DateTime = domain.DateTime,
            FromAirportId = domain.FromAirportId,
            ToAirportId = domain.ToAirportId,
            Price = domain.Price
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<FlightDomain> ToDomainList(IEnumerable<FlightPostgresqlModel> models)
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
    public static List<FlightPostgresqlModel> ToModelList(IEnumerable<FlightDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
