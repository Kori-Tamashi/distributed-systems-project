using core.domain;
using core.enums;
using dataaccess.models.postgres;

using TicketDomain = core.domain.Ticket;
using TicketPostgresqlModel = dataaccess.models.postgres.TicketPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Ticket domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
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
            FlightId = model.FlightId,
            PassengerName = model.PassengerName,
            PassengerEmail = model.PassengerEmail,
            PassengerPhone = model.PassengerPhone,
            SeatNumber = model.SeatNumber,
            Class = (TicketClass)model.Class,
            Price = model.Price,
            BookingDate = model.BookingDate,
            Status = (TicketStatus)model.Status,
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
            FlightId = domain.FlightId,
            PassengerName = domain.PassengerName,
            PassengerEmail = domain.PassengerEmail,
            PassengerPhone = domain.PassengerPhone,
            SeatNumber = domain.SeatNumber,
            Class = (int)domain.Class,
            Price = domain.Price,
            BookingDate = domain.BookingDate,
            Status = (int)domain.Status,
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
