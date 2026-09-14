using core.domain;
using core.enums;
using dataaccess.models.postgres;

using BookingDomain = core.domain.Booking;
using BookingPostgresqlModel = dataaccess.models.postgres.BookingPostgresqlModel;

namespace dataaccess.converters.postgres;

/// <summary>
/// Converter between Booking domain entity and PostgreSQL model
/// Handles mapping between domain layer and data access layer
/// </summary>
public static class BookingPostgresqlConverter
{
    /// <summary>
    /// Converts PostgreSQL model to domain entity
    /// </summary>
    /// <param name="model">PostgreSQL model to convert</param>
    /// <returns>Domain entity representation of Booking</returns>
    public static BookingDomain ToDomain(BookingPostgresqlModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new BookingDomain
        {
            Id = model.Id,
            BookingUid = model.BookingUid,
            BookingReference = model.BookingReference,
            CustomerName = model.CustomerName,
            CustomerEmail = model.CustomerEmail,
            CustomerPhone = model.CustomerPhone,
            TotalPrice = model.TotalPrice,
            BookingDate = model.BookingDate,
            Status = (BookingStatus)model.Status,
            PaymentMethod = (PaymentMethod)model.PaymentMethod,
            PaymentTransactionId = model.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts domain entity to PostgreSQL model
    /// </summary>
    /// <param name="domain">Domain entity to convert</param>
    /// <returns>PostgreSQL model representation of Booking</returns>
    public static BookingPostgresqlModel ToModel(BookingDomain domain)
    {
        if (domain == null)
            throw new ArgumentNullException(nameof(domain));

        return new BookingPostgresqlModel
        {
            Id = domain.Id,
            BookingUid = domain.BookingUid,
            BookingReference = domain.BookingReference,
            CustomerName = domain.CustomerName,
            CustomerEmail = domain.CustomerEmail,
            CustomerPhone = domain.CustomerPhone,
            TotalPrice = domain.TotalPrice,
            BookingDate = domain.BookingDate,
            Status = (int)domain.Status,
            PaymentMethod = (int)domain.PaymentMethod,
            PaymentTransactionId = domain.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts a collection of PostgreSQL models to domain entities
    /// </summary>
    /// <param name="models">Collection of PostgreSQL models</param>
    /// <returns>Collection of domain entities</returns>
    public static List<BookingDomain> ToDomainList(IEnumerable<BookingPostgresqlModel> models)
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
    public static List<BookingPostgresqlModel> ToModelList(IEnumerable<BookingDomain> domains)
    {
        if (domains == null)
            throw new ArgumentNullException(nameof(domains));

        return domains.Select(ToModel).ToList();
    }
}
