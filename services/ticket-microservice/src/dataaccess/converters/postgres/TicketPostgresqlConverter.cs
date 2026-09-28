using core.domain;
using dataaccess.models.postgres;

using TicketDomain = core.domain.Ticket;
using TicketPostgresqlModel = dataaccess.models.postgres.TicketPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Ticket domain entity and PostgreSQL model (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
/// </summary>
public static class TicketPostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of Ticket</returns>
    public static TicketDomain ToDomain(TicketPostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new TicketDomain
        {
            Id = model.Id,
            TicketUid = model.TicketUid,
            Username = model.Username,
            FlightNumber = model.FlightNumber,
            Price = model.Price,
            Status = model.Status,
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of Ticket</returns>
    public static TicketPostgresqlModel ToModel(TicketDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new TicketPostgresqlModel
        {
            Id = domain.Id,
            TicketUid = domain.TicketUid,
            Username = domain.Username,
            FlightNumber = domain.FlightNumber,
            Price = domain.Price,
            Status = domain.Status,
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<TicketDomain> ToDomainList(IEnumerable<TicketPostgresqlModel> models)
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
    public static List<TicketPostgresqlModel> ToModelList(IEnumerable<TicketDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
