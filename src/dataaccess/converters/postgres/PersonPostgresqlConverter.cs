using core.domain;
using dataaccess.models.postgres;

using PersonDomain = core.domain.Person;
using PersonPostgresqlModel = dataaccess.models.postgres.PersonPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Person domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
/// </summary>
public static class PersonPostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of Person</returns>
    public static PersonDomain ToDomain(PersonPostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new PersonDomain
        {
            Id = model.Id,
            Name = model.Name,
            Age = model.Age,
            Address = model.Address,
            Work = model.Work
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of Person</returns>
    public static PersonPostgresqlModel ToModel(PersonDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new PersonPostgresqlModel
        {
            Id = domain.Id,
            Name = domain.Name,
            Age = domain.Age,
            Address = domain.Address,
            Work = domain.Work
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<PersonDomain> ToDomainList(IEnumerable<PersonPostgresqlModel> models)
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
    public static List<PersonPostgresqlModel> ToModelList(IEnumerable<PersonDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
