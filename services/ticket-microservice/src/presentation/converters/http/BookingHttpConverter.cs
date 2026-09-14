using presentation.dto.http;
using presentation.dto.http.Booking;

namespace presentation.converters.http;

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
    /// Converts list of domain Bookings to list of BookingDTOs
    /// </summary>
    public static List<BookingDTO> ToDTO(List<core.domain.Booking> bookings)
    {
        return bookings.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts list of BookingDTOs to list of domain Bookings
    /// </summary>
    public static List<core.domain.Booking> ToDomain(List<BookingDTO> dtos)
    {
        return dtos.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts domain Booking to CreateBookingDTO
    /// </summary>
    public static CreateBookingDTO ToCreateDTO(core.domain.Booking booking)
    {
        return new CreateBookingDTO
        {
            BookingReference = booking.BookingReference,
            BookingUid = booking.BookingUid,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            BookingDate = booking.BookingDate,
            TotalPrice = booking.TotalPrice,
            PaymentMethod = (int)booking.PaymentMethod,
            PaymentTransactionId = booking.PaymentTransactionId
        };
    }

    /// <summary>
    /// Converts CreateBookingDTO to domain Booking
    /// </summary>
    public static core.domain.Booking ToCreateDomain(CreateBookingDTO dto)
    {
        return new core.domain.Booking
        {
            Id = 0,
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
    /// Converts domain Booking to UpdateBookingDTO
    /// </summary>
    public static UpdateBookingDTO ToUpdateDTO(core.domain.Booking booking)
    {
        return new UpdateBookingDTO(booking.Id)
        {
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
    /// Converts UpdateBookingDTO to domain Booking
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Booking ToUpdateDomain(UpdateBookingDTO dto, core.domain.Booking existingBooking)
    {
        existingBooking.BookingReference = dto.BookingReference ?? existingBooking.BookingReference;
        existingBooking.BookingUid = dto.BookingUid ?? existingBooking.BookingUid;
        existingBooking.CustomerName = dto.CustomerName ?? existingBooking.CustomerName;
        existingBooking.CustomerEmail = dto.CustomerEmail ?? existingBooking.CustomerEmail;
        existingBooking.CustomerPhone = dto.CustomerPhone ?? existingBooking.CustomerPhone;
        existingBooking.BookingDate = dto.BookingDate ?? existingBooking.BookingDate;
        existingBooking.TotalPrice = dto.TotalPrice ?? existingBooking.TotalPrice;
        existingBooking.Status = dto.Status.HasValue ? (core.enums.BookingStatus)dto.Status.Value : existingBooking.Status;
        existingBooking.PaymentMethod = dto.PaymentMethod.HasValue ? (core.enums.PaymentMethod)dto.PaymentMethod.Value : existingBooking.PaymentMethod;
        existingBooking.PaymentTransactionId = dto.PaymentTransactionId ?? existingBooking.PaymentTransactionId;

        return existingBooking;
    }

    /// <summary>
    /// Converts domain Booking to UpdateBookingDTO (without existing booking)
    /// </summary>
    public static UpdateBookingDTO ToUpdateDTOWithoutMerge(core.domain.Booking booking)
    {
        return new UpdateBookingDTO(booking.Id)
        {
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
    /// Converts domain Booking to UpdateBookingDTO with only changed properties
    /// </summary>
    public static UpdateBookingDTO ToUpdateDTOPartial(core.domain.Booking booking, params string[] changedProperties)
    {
        var dto = new UpdateBookingDTO(booking.Id);

        if (Array.Exists(changedProperties, p => p.Equals("BookingReference", StringComparison.OrdinalIgnoreCase)))
            dto.BookingReference = booking.BookingReference;

        if (Array.Exists(changedProperties, p => p.Equals("BookingUid", StringComparison.OrdinalIgnoreCase)))
            dto.BookingUid = booking.BookingUid;

        if (Array.Exists(changedProperties, p => p.Equals("CustomerName", StringComparison.OrdinalIgnoreCase)))
            dto.CustomerName = booking.CustomerName;

        if (Array.Exists(changedProperties, p => p.Equals("CustomerEmail", StringComparison.OrdinalIgnoreCase)))
            dto.CustomerEmail = booking.CustomerEmail;

        if (Array.Exists(changedProperties, p => p.Equals("CustomerPhone", StringComparison.OrdinalIgnoreCase)))
            dto.CustomerPhone = booking.CustomerPhone;

        if (Array.Exists(changedProperties, p => p.Equals("BookingDate", StringComparison.OrdinalIgnoreCase)))
            dto.BookingDate = booking.BookingDate;

        if (Array.Exists(changedProperties, p => p.Equals("TotalPrice", StringComparison.OrdinalIgnoreCase)))
            dto.TotalPrice = booking.TotalPrice;

        if (Array.Exists(changedProperties, p => p.Equals("Status", StringComparison.OrdinalIgnoreCase)))
            dto.Status = (int)booking.Status;

        if (Array.Exists(changedProperties, p => p.Equals("PaymentMethod", StringComparison.OrdinalIgnoreCase)))
            dto.PaymentMethod = (int)booking.PaymentMethod;

        if (Array.Exists(changedProperties, p => p.Equals("PaymentTransactionId", StringComparison.OrdinalIgnoreCase)))
            dto.PaymentTransactionId = booking.PaymentTransactionId;

        return dto;
    }

    /// <summary>
    /// Converts domain Booking to UpdateBookingDTO with only changed properties (nullable check)
    /// </summary>
    public static UpdateBookingDTO ToUpdateDTOPartialNullable(core.domain.Booking booking, params bool[] propertyChanged)
    {
        var dto = new UpdateBookingDTO(booking.Id);

        if (propertyChanged.Length >= 1 && propertyChanged[0])
            dto.BookingReference = booking.BookingReference;

        if (propertyChanged.Length >= 2 && propertyChanged[1])
            dto.BookingUid = booking.BookingUid;

        if (propertyChanged.Length >= 3 && propertyChanged[2])
            dto.CustomerName = booking.CustomerName;

        if (propertyChanged.Length >= 4 && propertyChanged[3])
            dto.CustomerEmail = booking.CustomerEmail;

        if (propertyChanged.Length >= 5 && propertyChanged[4])
            dto.CustomerPhone = booking.CustomerPhone;

        if (propertyChanged.Length >= 6 && propertyChanged[5])
            dto.BookingDate = booking.BookingDate;

        if (propertyChanged.Length >= 7 && propertyChanged[6])
            dto.TotalPrice = booking.TotalPrice;

        if (propertyChanged.Length >= 8 && propertyChanged[7])
            dto.Status = (int)booking.Status;

        if (propertyChanged.Length >= 9 && propertyChanged[8])
            dto.PaymentMethod = (int)booking.PaymentMethod;

        if (propertyChanged.Length >= 10 && propertyChanged[9])
            dto.PaymentTransactionId = booking.PaymentTransactionId;

        return dto;
    }
}
