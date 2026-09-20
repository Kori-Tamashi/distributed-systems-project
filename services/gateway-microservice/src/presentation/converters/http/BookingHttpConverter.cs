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
        if (booking == null) return null!;
        
        return new BookingDTO
        {
            Id = booking.Id,
            BookingUid = booking.BookingUid,
            BookingReference = booking.BookingReference,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            Status = (int)booking.Status,
            PaymentMethod = (int)booking.PaymentMethod,
            PaymentTransactionId = booking.PaymentTransactionId,
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
            BookingUid = dto.BookingUid,
            BookingReference = dto.BookingReference,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            TotalPrice = dto.TotalPrice,
            BookingDate = dto.BookingDate,
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
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            Status = (int)booking.Status,
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
            BookingUid = Guid.NewGuid(),
            BookingReference = dto.BookingReference,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            TotalPrice = dto.TotalPrice,
            BookingDate = dto.BookingDate,
            Status = (core.enums.BookingStatus)dto.Status,
            PaymentMethod = (core.enums.PaymentMethod)dto.PaymentMethod,
            PaymentTransactionId = dto.PaymentTransactionId,
        };
    }

    /// <summary>
    /// Converts domain Booking to UpdateBookingDTO
    /// </summary>
    public static UpdateBookingDTO ToUpdateDTO(core.domain.Booking booking)
    {
        return new UpdateBookingDTO
        {
            BookingReference = booking.BookingReference,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
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
        if (dto == null || existingBooking == null) return existingBooking;
        
        existingBooking.BookingReference = dto.BookingReference ?? existingBooking.BookingReference;
        existingBooking.CustomerName = dto.CustomerName ?? existingBooking.CustomerName;
        existingBooking.CustomerEmail = dto.CustomerEmail ?? existingBooking.CustomerEmail;
        existingBooking.CustomerPhone = dto.CustomerPhone ?? existingBooking.CustomerPhone;
        existingBooking.TotalPrice = dto.TotalPrice ?? existingBooking.TotalPrice;
        existingBooking.BookingDate = dto.BookingDate ?? existingBooking.BookingDate;
        existingBooking.Status = dto.Status.HasValue ? (core.enums.BookingStatus)dto.Status.Value : existingBooking.Status;
        existingBooking.PaymentMethod = dto.PaymentMethod.HasValue ? (core.enums.PaymentMethod)dto.PaymentMethod.Value : existingBooking.PaymentMethod;
        existingBooking.PaymentTransactionId = dto.PaymentTransactionId ?? existingBooking.PaymentTransactionId;

        return existingBooking;
    }
}
