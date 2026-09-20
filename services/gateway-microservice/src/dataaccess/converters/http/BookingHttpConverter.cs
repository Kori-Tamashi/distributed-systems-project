using dataaccess.dto.http;
using dataaccess.dto.http.Booking;

namespace dataaccess.converters.http;

/// <summary>
/// Converter for Booking between domain and HTTP DTO representations
/// </summary>
public static class BookingHttpConverter
{
    /// <summary>
    /// Converts domain Booking to BookingDTO
    /// </summary>
    public static BookingDTO ToDTO(core.domain.Booking booking)
    {
        return new BookingDTO
        {
            Id = booking.Id,
            BookingReference = booking.BookingReference,
            BookingUid = booking.BookingUid,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            Status = (int)booking.Status,
            PaymentMethod = (int)booking.PaymentMethod,
            PaymentTransactionId = booking.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts BookingDTO to domain Booking
    /// </summary>
    public static core.domain.Booking ToDomain(BookingDTO dto)
    {
        return new core.domain.Booking
        {
            Id = dto.Id,
            BookingReference = dto.BookingReference,
            BookingUid = dto.BookingUid,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            BookingDate = dto.BookingDate,
            TotalPrice = dto.TotalPrice,
            Status = (core.enums.BookingStatus)dto.Status,
            PaymentMethod = (core.enums.PaymentMethod)dto.PaymentMethod,
            PaymentTransactionId = dto.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts CreateBookingDTO to domain Booking
    /// </summary>
    public static core.domain.Booking ToDomain(CreateBookingDTO dto)
    {
        return new core.domain.Booking
        {
            Id = dto.Id,
            BookingReference = dto.BookingReference,
            BookingUid = dto.BookingUid,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            BookingDate = dto.BookingDate,
            TotalPrice = dto.TotalPrice,
            Status = core.enums.BookingStatus.Confirmed,
            PaymentMethod = (core.enums.PaymentMethod)dto.PaymentMethod,
            PaymentTransactionId = dto.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts UpdateBookingDTO to domain Booking
    /// </summary>
    public static core.domain.Booking ToDomain(UpdateBookingDTO dto)
    {
        return new core.domain.Booking
        {
            Id = dto.Id,
            BookingReference = dto.BookingReference ?? string.Empty,
            BookingUid = dto.BookingUid ?? Guid.Empty,
            CustomerName = dto.CustomerName ?? string.Empty,
            CustomerEmail = dto.CustomerEmail ?? string.Empty,
            CustomerPhone = dto.CustomerPhone ?? string.Empty,
            BookingDate = dto.BookingDate ?? DateTime.MinValue,
            TotalPrice = dto.TotalPrice ?? 0,
            Status = dto.Status.HasValue ? (core.enums.BookingStatus)dto.Status.Value : core.enums.BookingStatus.Confirmed,
            PaymentMethod = dto.PaymentMethod.HasValue ? (core.enums.PaymentMethod)dto.PaymentMethod.Value : core.enums.PaymentMethod.Cash,
            PaymentTransactionId = dto.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts domain Booking to UpdateBookingDTO
    /// </summary>
    public static UpdateBookingDTO ToUpdateDTO(core.domain.Booking booking)
    {
        return new UpdateBookingDTO
        {
            Id = booking.Id,
            BookingReference = booking.BookingReference,
            BookingUid = booking.BookingUid,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            Status = (int?)booking.Status,
            PaymentMethod = (int?)booking.PaymentMethod,
            PaymentTransactionId = booking.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts list of domain Bookings to list of BookingDTOs
    /// </summary>
    public static List<BookingDTO> ToDTO(List<core.domain.Booking> bookings)
    {
        return bookings.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts IEnumerable of domain Bookings to IEnumerable of BookingDTOs
    /// </summary>
    public static IEnumerable<BookingDTO> ToDTO(IEnumerable<core.domain.Booking> bookings)
    {
        return bookings.Select(ToDTO);
    }
}
