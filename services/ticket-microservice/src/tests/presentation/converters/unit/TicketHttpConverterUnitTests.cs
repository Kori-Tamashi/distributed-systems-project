using core.domain;
using core.enums;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Ticket;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for TicketHttpConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid ticket with all fields (normal case)
/// - EP2: Ticket with minimal data
/// - EP3: Ticket with maximum values
/// - EP4: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid ticket to create DTO
/// - EP2: Create DTO should have Id = 0
/// - EP3: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid ticket to update DTO
/// - EP2: Update DTO preserves ID
/// - EP3: UpdateDomain merges nullable properties correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of tickets
/// - EP2: Empty list handling
/// 
/// Total: 12 unit tests (all should pass)
/// </summary>
public class TicketHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid ticket with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ValidTicketWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var dto = TicketHttpConverter.ToDTO(ticket);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(ticket.Id, dto.Id);
        Assert.Equal(ticket.TicketUid, dto.TicketUid);
        Assert.Equal(ticket.FlightId, dto.FlightId);
        Assert.Equal(ticket.PassengerName, dto.PassengerName);
        Assert.Equal(ticket.PassengerEmail, dto.PassengerEmail);
        Assert.Equal(ticket.PassengerPhone, dto.PassengerPhone);
        Assert.Equal(ticket.SeatNumber, dto.SeatNumber);
        Assert.Equal((int)ticket.Class, dto.Class);
        Assert.Equal(ticket.Price, dto.Price);
        Assert.Equal(ticket.BookingDate, dto.BookingDate);
        Assert.Equal((int)ticket.Status, dto.Status);
    }

    /// <summary>
    /// EP2: Ticket with minimal data - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_MinimalTicket_ShouldConvertCorrectly()
    {
        // Arrange
        var ticket = TicketMother.CreateMinimalTicket();

        // Act
        var dto = TicketHttpConverter.ToDTO(ticket);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(ticket.Id, dto.Id);
        Assert.Equal(ticket.PassengerName, dto.PassengerName);
        Assert.Equal(ticket.Price, dto.Price);
    }

    /// <summary>
    /// EP3: Ticket with maximum values - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_TicketWithMaxValues_ShouldConvertCorrectly()
    {
        // Arrange
        var ticket = TicketMother.CreateTicketWithMaxPrice();

        // Act
        var dto = TicketHttpConverter.ToDTO(ticket);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(ticket.Id, dto.Id);
        Assert.Equal(int.MaxValue, dto.Price);
    }

    /// <summary>
    /// EP4: ToDomain should convert DTO back to domain entity
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ValidDto_ShouldConvertToDomainEntity()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        var dto = new TicketDTO
        {
            Id = ticket.Id,
            TicketUid = ticket.TicketUid,
            FlightId = ticket.FlightId,
            PassengerName = ticket.PassengerName,
            PassengerEmail = ticket.PassengerEmail,
            PassengerPhone = ticket.PassengerPhone,
            SeatNumber = ticket.SeatNumber,
            Class = (int)ticket.Class,
            Price = ticket.Price,
            BookingDate = ticket.BookingDate,
            Status = (int)ticket.Status
        };

        // Act
        var result = TicketHttpConverter.ToDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.TicketUid, result.TicketUid);
        Assert.Equal(dto.FlightId, result.FlightId);
        Assert.Equal(dto.PassengerName, result.PassengerName);
        Assert.Equal(dto.PassengerEmail, result.PassengerEmail);
        Assert.Equal(dto.PassengerPhone, result.PassengerPhone);
        Assert.Equal(dto.SeatNumber, result.SeatNumber);
        Assert.Equal((TicketClass)dto.Class, result.Class);
        Assert.Equal(dto.Price, result.Price);
        Assert.Equal(dto.BookingDate, result.BookingDate);
        Assert.Equal((TicketStatus)dto.Status, result.Status);
    }

    /// <summary>
    /// EP5: Round-trip conversion should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var dto = TicketHttpConverter.ToDTO(ticket);
        var result = TicketHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.TicketUid, result.TicketUid);
        Assert.Equal(ticket.FlightId, result.FlightId);
        Assert.Equal(ticket.PassengerName, result.PassengerName);
        Assert.Equal(ticket.PassengerEmail, result.PassengerEmail);
        Assert.Equal(ticket.PassengerPhone, result.PassengerPhone);
        Assert.Equal(ticket.SeatNumber, result.SeatNumber);
        Assert.Equal(ticket.Class, result.Class);
        Assert.Equal(ticket.Price, result.Price);
        Assert.Equal(ticket.BookingDate, result.BookingDate);
        Assert.Equal(ticket.Status, result.Status);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid ticket to create DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidTicket_ShouldMapAllFields()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var dto = TicketHttpConverter.ToCreateDTO(ticket);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(ticket.TicketUid, dto.TicketUid);
        Assert.Equal(ticket.FlightId, dto.FlightId);
        Assert.Equal(ticket.PassengerName, dto.PassengerName);
        Assert.Equal(ticket.PassengerEmail, dto.PassengerEmail);
        Assert.Equal(ticket.PassengerPhone, dto.PassengerPhone);
        Assert.Equal(ticket.SeatNumber, dto.SeatNumber);
        Assert.Equal((int)ticket.Class, dto.Class);
        Assert.Equal(ticket.Price, dto.Price);
        Assert.Equal(ticket.BookingDate, dto.BookingDate);
        Assert.Equal((int)ticket.Status, dto.Status);
    }

    /// <summary>
    /// EP2: ToCreateDomain should set Id to 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldSetIdToZero()
    {
        // Arrange
        var dto = new CreateTicketDTO
        {
            TicketUid = Guid.NewGuid(),
            FlightId = 1,
            PassengerName = "John Doe",
            PassengerEmail = "john@example.com",
            PassengerPhone = "+79000000000",
            SeatNumber = "12A",
            Class = (int)TicketClass.Economy,
            Price = 15000,
            BookingDate = DateTime.UtcNow.AddDays(-1),
            Status = (int)TicketStatus.Confirmed
        };

        // Act
        var result = TicketHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(dto.TicketUid, result.TicketUid);
        Assert.Equal(dto.FlightId, result.FlightId);
    }

    /// <summary>
    /// EP3: Create DTO round-trip should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var dto = TicketHttpConverter.ToCreateDTO(ticket);
        var result = TicketHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(ticket.TicketUid, result.TicketUid);
        Assert.Equal(ticket.FlightId, result.FlightId);
        Assert.Equal(ticket.PassengerName, result.PassengerName);
        Assert.Equal(ticket.PassengerEmail, result.PassengerEmail);
        Assert.Equal(ticket.PassengerPhone, result.PassengerPhone);
        Assert.Equal(ticket.SeatNumber, result.SeatNumber);
        Assert.Equal(ticket.Class, result.Class);
        Assert.Equal(ticket.Price, result.Price);
        Assert.Equal(ticket.BookingDate, result.BookingDate);
        Assert.Equal(ticket.Status, result.Status);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid ticket to update DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidTicket_ShouldMapAllFields()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var dto = TicketHttpConverter.ToUpdateDTO(ticket);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(ticket.Id, dto.Id);
        Assert.Equal(ticket.TicketUid, dto.TicketUid);
        Assert.Equal(ticket.FlightId, dto.FlightId);
        Assert.Equal(ticket.PassengerName, dto.PassengerName);
        Assert.Equal(ticket.PassengerEmail, dto.PassengerEmail);
        Assert.Equal(ticket.PassengerPhone, dto.PassengerPhone);
        Assert.Equal(ticket.SeatNumber, dto.SeatNumber);
        Assert.Equal((int)ticket.Class, dto.Class);
        Assert.Equal(ticket.Price, dto.Price);
        Assert.Equal(ticket.BookingDate, dto.BookingDate);
        Assert.Equal((int)ticket.Status, dto.Status);
    }

    /// <summary>
    /// EP2: ToUpdateDomain should preserve ID
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveId()
    {
        // Arrange
        var existingTicket = TicketMother.CreateValidTicket();
        var updateDto = new UpdateTicketDTO(existingTicket.Id)
        {
            PassengerName = "Jane Doe"
        };

        // Act
        var result = TicketHttpConverter.ToUpdateDomain(updateDto, existingTicket);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingTicket.Id, result.Id);
        Assert.Equal("Jane Doe", result.PassengerName);
    }

    /// <summary>
    /// EP3: UpdateDomain merges nullable properties correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_WithPartialUpdate_ShouldMergeCorrectly()
    {
        // Arrange
        var existingTicket = TicketMother.CreateValidTicket();
        var originalEmail = existingTicket.PassengerEmail;
        var originalPrice = existingTicket.Price;
        
        var updateDto = new UpdateTicketDTO(existingTicket.Id)
        {
            PassengerName = "Jane Doe",
            Price = 20000
        };

        // Act
        var result = TicketHttpConverter.ToUpdateDomain(updateDto, existingTicket);

        // Assert
        Assert.Equal(existingTicket.Id, result.Id);
        Assert.Equal("Jane Doe", result.PassengerName);
        Assert.Equal(20000, result.Price);
        Assert.Equal(originalEmail, result.PassengerEmail); // Should remain unchanged
    }

    #endregion

    #region List Conversion Tests

    /// <summary>
    /// EP1: Convert list of tickets
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfTickets_ShouldConvertAll()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
        for (int i = 0; i < tickets.Count; i++)
        {
            Assert.Equal(tickets[i].Id, dtos[i].Id);
            Assert.Equal(tickets[i].TicketUid, dtos[i].TicketUid);
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
        var tickets = new List<Ticket>();

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets);

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
