using core.domain;
using core.enums;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Booking;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for BookingHttpConverter
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
        Assert.Equal(booking.BookingReference, dto.BookingReference);
        Assert.Equal(booking.BookingUid, dto.BookingUid);
        Assert.Equal(booking.CustomerName, dto.CustomerName);
        Assert.Equal(booking.CustomerEmail, dto.CustomerEmail);
        Assert.Equal(booking.CustomerPhone, dto.CustomerPhone);
        Assert.Equal(booking.BookingDate, dto.BookingDate);
        Assert.Equal(booking.TotalPrice, dto.TotalPrice);
        Assert.Equal((int)booking.Status, dto.Status);
        Assert.Equal((int)booking.PaymentMethod, dto.PaymentMethod);
        Assert.Equal(booking.PaymentTransactionId, dto.PaymentTransactionId);
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
        Assert.Equal(originalBooking.BookingReference, convertedBooking.BookingReference);
        Assert.Equal(originalBooking.BookingUid, convertedBooking.BookingUid);
        Assert.Equal(originalBooking.CustomerName, convertedBooking.CustomerName);
        Assert.Equal(originalBooking.CustomerEmail, convertedBooking.CustomerEmail);
        Assert.Equal(originalBooking.CustomerPhone, convertedBooking.CustomerPhone);
        Assert.Equal(originalBooking.BookingDate, convertedBooking.BookingDate);
        Assert.Equal(originalBooking.TotalPrice, convertedBooking.TotalPrice);
        Assert.Equal(originalBooking.Status, convertedBooking.Status);
        Assert.Equal(originalBooking.PaymentMethod, convertedBooking.PaymentMethod);
        Assert.Equal(originalBooking.PaymentTransactionId, convertedBooking.PaymentTransactionId);
    }

    #endregion

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_CreateBookingDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreateBookingDTO
        {
            BookingReference = "BK500",
            BookingUid = Guid.NewGuid(),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+79001234567",
            BookingDate = DateTime.UtcNow,
            TotalPrice = 25000,
            PaymentMethod = (int)PaymentMethod.Cash,
            PaymentTransactionId = "TXN999"
        };

        // Act
        var booking = BookingHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.BookingReference, booking.BookingReference);
        Assert.Equal(createDto.CustomerName, booking.CustomerName);
        Assert.Equal(createDto.TotalPrice, booking.TotalPrice);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToDomain_CreateBookingDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalBooking = BookingMother.CreateValidBooking();
        var createDto = new CreateBookingDTO
        {
            BookingReference = originalBooking.BookingReference,
            BookingUid = originalBooking.BookingUid,
            CustomerName = originalBooking.CustomerName,
            CustomerEmail = originalBooking.CustomerEmail,
            CustomerPhone = originalBooking.CustomerPhone,
            BookingDate = originalBooking.BookingDate,
            TotalPrice = originalBooking.TotalPrice,
            PaymentMethod = (int)originalBooking.PaymentMethod,
            PaymentTransactionId = originalBooking.PaymentTransactionId
        };

        // Act
        var booking = BookingHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalBooking.BookingReference, booking.BookingReference);
        Assert.Equal(originalBooking.CustomerName, booking.CustomerName);
        Assert.Equal(originalBooking.TotalPrice, booking.TotalPrice);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdateBookingDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdateBookingDTO(1)
        {
            CustomerName = "Updated Name",
            TotalPrice = 30000
        };

        // Act
        var booking = BookingHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, booking.Id);
        Assert.Equal("Updated Name", booking.CustomerName);
        Assert.Equal(30000, booking.TotalPrice);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdateBookingDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdateBookingDTO(1);

        // Act
        var booking = BookingHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, booking.Id);
        Assert.Equal(string.Empty, booking.CustomerName);
        Assert.Equal(0, booking.TotalPrice);
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
        var bookings = BookingMother.CreateBookingList(5);

        // Act
        var dtos = BookingHttpConverter.ToDTO(bookings).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
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
        var bookings = new List<Booking>();

        // Act
        var dtos = BookingHttpConverter.ToDTO(bookings).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
