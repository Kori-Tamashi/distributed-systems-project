using core.enums;
using presentation.converters.http;
using presentation.dto.http.Booking;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation BookingHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid booking with all fields (normal case)
/// - EP2: Booking with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid booking to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid booking to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of bookings
/// - EP2: Empty list handling
/// 
/// Total: 10 unit tests (all should pass)
/// </summary>
public class BookingHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid booking with all fields - should convert correctly
    /// </summary>
    [Fact]
    public void ToDTO_ValidBookingWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var dto = BookingHttpConverter.ToDTO(booking);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(booking.Id, dto.Id);
        Assert.Equal(booking.BookingUid, dto.BookingUid);
        Assert.Equal(booking.BookingReference, dto.BookingReference);
        Assert.Equal(booking.CustomerName, dto.CustomerName);
        Assert.Equal(booking.CustomerEmail, dto.CustomerEmail);
        Assert.Equal(booking.CustomerPhone, dto.CustomerPhone);
        Assert.Equal(booking.TotalPrice, dto.TotalPrice);
        Assert.Equal(booking.BookingDate, dto.BookingDate);
        Assert.Equal((int)booking.Status, dto.Status);
        Assert.Equal((int)booking.PaymentMethod, dto.PaymentMethod);
    }

    /// <summary>
    /// EP2: Booking with minimal data - should handle correctly
    /// </summary>
    [Fact]
    public void ToDTO_MinimalBooking_ShouldConvertCorrectly()
    {
        // Arrange
        var booking = BookingMother.CreateMinimalBooking();

        // Act
        var dto = BookingHttpConverter.ToDTO(booking);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(booking.Id, dto.Id);
        Assert.Equal(booking.CustomerName, dto.CustomerName);
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
    [Fact]
    public void ToDTO_ToDomain_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalBooking = BookingMother.CreateValidBooking();

        // Act
        var dto = BookingHttpConverter.ToDTO(originalBooking);
        var convertedBooking = BookingHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(originalBooking.Id, convertedBooking.Id);
        Assert.Equal(originalBooking.BookingUid, convertedBooking.BookingUid);
        Assert.Equal(originalBooking.BookingReference, convertedBooking.BookingReference);
        Assert.Equal(originalBooking.CustomerName, convertedBooking.CustomerName);
        Assert.Equal(originalBooking.CustomerEmail, convertedBooking.CustomerEmail);
        Assert.Equal(originalBooking.CustomerPhone, convertedBooking.CustomerPhone);
        Assert.Equal(originalBooking.TotalPrice, convertedBooking.TotalPrice);
        Assert.Equal(originalBooking.BookingDate, convertedBooking.BookingDate);
        Assert.Equal(originalBooking.Status, convertedBooking.Status);
        Assert.Equal(originalBooking.PaymentMethod, convertedBooking.PaymentMethod);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToCreateDTO_ValidBooking_ShouldConvertCorrectly()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var createDto = BookingHttpConverter.ToCreateDTO(booking);

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal(booking.BookingReference, createDto.BookingReference);
        Assert.Equal(booking.CustomerName, createDto.CustomerName);
        Assert.Equal(booking.TotalPrice, createDto.TotalPrice);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToCreateDomain_CreateBookingDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalBooking = BookingMother.CreateValidBooking();
        var createDto = new CreateBookingDTO
        {
            BookingReference = originalBooking.BookingReference,
            CustomerName = originalBooking.CustomerName,
            CustomerEmail = originalBooking.CustomerEmail,
            CustomerPhone = originalBooking.CustomerPhone,
            TotalPrice = originalBooking.TotalPrice,
            BookingDate = originalBooking.BookingDate,
            Status = (int)originalBooking.Status,
            PaymentMethod = (int)originalBooking.PaymentMethod
        };

        // Act
        var booking = BookingHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalBooking.BookingReference, booking.BookingReference);
        Assert.Equal(originalBooking.CustomerName, booking.CustomerName);
        Assert.Equal(originalBooking.TotalPrice, booking.TotalPrice);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToUpdateDTO_ValidBooking_ShouldConvertCorrectly()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var updateDto = BookingHttpConverter.ToUpdateDTO(booking);

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(booking.BookingReference, updateDto.BookingReference);
        Assert.Equal(booking.CustomerName, updateDto.CustomerName);
        Assert.Equal(booking.TotalPrice, updateDto.TotalPrice);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdateBookingDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingBooking = BookingMother.CreateValidBooking();
        var updateDto = new UpdateBookingDTO
        {
            CustomerName = "Updated Name"
            // Other fields are null
        };

        // Act
        var updatedBooking = BookingHttpConverter.ToUpdateDomain(updateDto, existingBooking);

        // Assert
        Assert.Equal("Updated Name", updatedBooking.CustomerName);
        Assert.Equal(existingBooking.BookingReference, updatedBooking.BookingReference);
        Assert.Equal(existingBooking.TotalPrice, updatedBooking.TotalPrice);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of bookings
    /// </summary>
    [Fact]
    public void ToDTO_ListOfBookings_ShouldConvertAll()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(3);

        // Act
        var dtos = BookingHttpConverter.ToDTO(bookings).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < bookings.Count; i++)
        {
            Assert.Equal(bookings[i].Id, dtos[i].Id);
            Assert.Equal(bookings[i].CustomerName, dtos[i].CustomerName);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    [Fact]
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var bookings = new List<core.domain.Booking>();

        // Act
        var dtos = BookingHttpConverter.ToDTO(bookings).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
