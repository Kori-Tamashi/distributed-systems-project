using core.domain;
using core.enums;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Booking;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for BookingHttpConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid booking with all fields (normal case)
/// - EP2: Booking with minimal data
/// - EP3: Booking with maximum values
/// - EP4: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid booking to create DTO
/// - EP2: Create DTO should have Id = 0
/// - EP3: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid booking to update DTO
/// - EP2: Update DTO preserves ID
/// - EP3: UpdateDomain merges nullable properties correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of bookings
/// - EP2: Empty list handling
/// 
/// Total: 12 unit tests (all should pass)
/// </summary>
public class BookingHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid booking with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
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
    [Unit]
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
        Assert.Equal(booking.TotalPrice, dto.TotalPrice);
    }

    /// <summary>
    /// EP3: Booking with maximum values - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_BookingWithMaxValues_ShouldConvertCorrectly()
    {
        // Arrange
        var booking = BookingMother.CreateBookingWithMaxPrice();

        // Act
        var dto = BookingHttpConverter.ToDTO(booking);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(booking.Id, dto.Id);
        Assert.Equal(int.MaxValue, dto.TotalPrice);
    }

    /// <summary>
    /// EP4: ToDomain should convert DTO back to domain entity
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ValidDto_ShouldConvertToDomainEntity()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        var dto = new BookingDTO
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

        // Act
        var result = BookingHttpConverter.ToDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.BookingReference, result.BookingReference);
        Assert.Equal(dto.BookingUid, result.BookingUid);
        Assert.Equal(dto.CustomerName, result.CustomerName);
        Assert.Equal(dto.CustomerEmail, result.CustomerEmail);
        Assert.Equal(dto.CustomerPhone, result.CustomerPhone);
        Assert.Equal(dto.BookingDate, result.BookingDate);
        Assert.Equal(dto.TotalPrice, result.TotalPrice);
        Assert.Equal((BookingStatus)dto.Status, result.Status);
        Assert.Equal((PaymentMethod)dto.PaymentMethod, result.PaymentMethod);
        Assert.Equal(dto.PaymentTransactionId, result.PaymentTransactionId);
    }

    /// <summary>
    /// EP5: Round-trip conversion should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var dto = BookingHttpConverter.ToDTO(booking);
        var result = BookingHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.BookingReference, result.BookingReference);
        Assert.Equal(booking.BookingUid, result.BookingUid);
        Assert.Equal(booking.CustomerName, result.CustomerName);
        Assert.Equal(booking.CustomerEmail, result.CustomerEmail);
        Assert.Equal(booking.CustomerPhone, result.CustomerPhone);
        Assert.Equal(booking.BookingDate, result.BookingDate);
        Assert.Equal(booking.TotalPrice, result.TotalPrice);
        Assert.Equal(booking.Status, result.Status);
        Assert.Equal(booking.PaymentMethod, result.PaymentMethod);
        Assert.Equal(booking.PaymentTransactionId, result.PaymentTransactionId);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid booking to create DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidBooking_ShouldMapAllFields()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var dto = BookingHttpConverter.ToCreateDTO(booking);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(booking.BookingReference, dto.BookingReference);
        Assert.Equal(booking.BookingUid, dto.BookingUid);
        Assert.Equal(booking.CustomerName, dto.CustomerName);
        Assert.Equal(booking.CustomerEmail, dto.CustomerEmail);
        Assert.Equal(booking.CustomerPhone, dto.CustomerPhone);
        Assert.Equal(booking.BookingDate, dto.BookingDate);
        Assert.Equal(booking.TotalPrice, dto.TotalPrice);
        Assert.Equal((int)booking.PaymentMethod, dto.PaymentMethod);
        Assert.Equal(booking.PaymentTransactionId, dto.PaymentTransactionId);
    }

    /// <summary>
    /// EP2: ToCreateDomain should set Id to 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldSetIdToZero()
    {
        // Arrange
        var dto = new CreateBookingDTO
        {
            BookingReference = "BK123",
            BookingUid = Guid.NewGuid(),
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerPhone = "+79000000000",
            BookingDate = DateTime.UtcNow.AddDays(-1),
            TotalPrice = 50000,
            PaymentMethod = (int)PaymentMethod.CreditCard,
            PaymentTransactionId = "TXN123"
        };

        // Act
        var result = BookingHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(dto.BookingReference, result.BookingReference);
    }

    /// <summary>
    /// EP3: Create DTO round-trip should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var dto = BookingHttpConverter.ToCreateDTO(booking);
        var result = BookingHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(booking.BookingReference, result.BookingReference);
        Assert.Equal(booking.BookingUid, result.BookingUid);
        Assert.Equal(booking.CustomerName, result.CustomerName);
        Assert.Equal(booking.CustomerEmail, result.CustomerEmail);
        Assert.Equal(booking.CustomerPhone, result.CustomerPhone);
        Assert.Equal(booking.BookingDate, result.BookingDate);
        Assert.Equal(booking.TotalPrice, result.TotalPrice);
        Assert.Equal(booking.PaymentMethod, result.PaymentMethod);
        Assert.Equal(booking.PaymentTransactionId, result.PaymentTransactionId);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid booking to update DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidBooking_ShouldMapAllFields()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();

        // Act
        var dto = BookingHttpConverter.ToUpdateDTO(booking);

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
    /// EP2: ToUpdateDomain should preserve ID
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveId()
    {
        // Arrange
        var existingBooking = BookingMother.CreateValidBooking();
        var updateDto = new UpdateBookingDTO(existingBooking.Id)
        {
            CustomerName = "Jane Doe"
        };

        // Act
        var result = BookingHttpConverter.ToUpdateDomain(updateDto, existingBooking);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingBooking.Id, result.Id);
        Assert.Equal("Jane Doe", result.CustomerName);
    }

    /// <summary>
    /// EP3: UpdateDomain merges nullable properties correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_WithPartialUpdate_ShouldMergeCorrectly()
    {
        // Arrange
        var existingBooking = BookingMother.CreateValidBooking();
        var originalEmail = existingBooking.CustomerEmail;
        var originalPrice = existingBooking.TotalPrice;
        
        var updateDto = new UpdateBookingDTO(existingBooking.Id)
        {
            CustomerName = "Jane Doe",
            TotalPrice = 60000
        };

        // Act
        var result = BookingHttpConverter.ToUpdateDomain(updateDto, existingBooking);

        // Assert
        Assert.Equal(existingBooking.Id, result.Id);
        Assert.Equal("Jane Doe", result.CustomerName);
        Assert.Equal(60000, result.TotalPrice);
        Assert.Equal(originalEmail, result.CustomerEmail); // Should remain unchanged
    }

    #endregion

    #region List Conversion Tests

    /// <summary>
    /// EP1: Convert list of bookings
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfBookings_ShouldConvertAll()
    {
        // Arrange
        var bookings = BookingMother.CreateBookingList(5);

        // Act
        var dtos = BookingHttpConverter.ToDTO(bookings);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
        for (int i = 0; i < bookings.Count; i++)
        {
            Assert.Equal(bookings[i].Id, dtos[i].Id);
            Assert.Equal(bookings[i].BookingUid, dtos[i].BookingUid);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_EmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var bookings = new List<Booking>();

        // Act
        var dtos = BookingHttpConverter.ToDTO(bookings);

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
