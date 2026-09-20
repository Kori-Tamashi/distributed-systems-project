using core.domain;
using core.enums;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Ticket;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for TicketHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid ticket with all fields (normal case)
/// - EP2: Ticket with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid ticket to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid ticket to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of tickets
/// - EP2: Empty list handling
/// 
/// Total: 10 unit tests (all should pass)
/// </summary>
public class TicketHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid ticket with all fields - should convert correctly
    /// </summary>
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
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
    public void ToDTO_ToDomain_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalTicket = TicketMother.CreateValidTicket();

        // Act
        var dto = TicketHttpConverter.ToDTO(originalTicket);
        var convertedTicket = TicketHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(originalTicket.Id, convertedTicket.Id);
        Assert.Equal(originalTicket.TicketUid, convertedTicket.TicketUid);
        Assert.Equal(originalTicket.FlightId, convertedTicket.FlightId);
        Assert.Equal(originalTicket.PassengerName, convertedTicket.PassengerName);
        Assert.Equal(originalTicket.PassengerEmail, convertedTicket.PassengerEmail);
        Assert.Equal(originalTicket.PassengerPhone, convertedTicket.PassengerPhone);
        Assert.Equal(originalTicket.SeatNumber, convertedTicket.SeatNumber);
        Assert.Equal(originalTicket.Class, convertedTicket.Class);
        Assert.Equal(originalTicket.Price, convertedTicket.Price);
        Assert.Equal(originalTicket.BookingDate, convertedTicket.BookingDate);
        Assert.Equal(originalTicket.Status, convertedTicket.Status);
    }

    #endregion

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    public void ToDomain_CreateTicketDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreateTicketDTO
        {
            TicketUid = Guid.NewGuid(),
            FlightId = 1,
            PassengerName = "Test Passenger",
            PassengerEmail = "test@example.com",
            PassengerPhone = "+79001234567",
            SeatNumber = "10A",
            Class = (int)TicketClass.Economy,
            Price = 15000,
            BookingDate = DateTime.UtcNow,
            Status = (int)TicketStatus.Confirmed
        };

        // Act
        var ticket = TicketHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.TicketUid, ticket.TicketUid);
        Assert.Equal(createDto.FlightId, ticket.FlightId);
        Assert.Equal(createDto.PassengerName, ticket.PassengerName);
        Assert.Equal(createDto.Price, ticket.Price);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    public void ToDomain_CreateTicketDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalTicket = TicketMother.CreateValidTicket();
        var createDto = new CreateTicketDTO
        {
            TicketUid = originalTicket.TicketUid,
            FlightId = originalTicket.FlightId,
            PassengerName = originalTicket.PassengerName,
            PassengerEmail = originalTicket.PassengerEmail,
            PassengerPhone = originalTicket.PassengerPhone,
            SeatNumber = originalTicket.SeatNumber,
            Class = (int)originalTicket.Class,
            Price = originalTicket.Price,
            BookingDate = originalTicket.BookingDate,
            Status = (int)originalTicket.Status
        };

        // Act
        var ticket = TicketHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalTicket.TicketUid, ticket.TicketUid);
        Assert.Equal(originalTicket.PassengerName, ticket.PassengerName);
        Assert.Equal(originalTicket.Price, ticket.Price);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    public void ToDomain_UpdateTicketDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdateTicketDTO(1)
        {
            PassengerName = "Updated Name",
            Price = 20000
        };

        // Act
        var ticket = TicketHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, ticket.Id);
        Assert.Equal("Updated Name", ticket.PassengerName);
        Assert.Equal(20000, ticket.Price);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    public void ToDomain_UpdateTicketDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdateTicketDTO(1);

        // Act
        var ticket = TicketHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, ticket.Id);
        Assert.Equal(string.Empty, ticket.PassengerName);
        Assert.Equal(0, ticket.Price);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of tickets
    /// </summary>
    public void ToDTO_ListOfTickets_ShouldConvertAll()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
        for (int i = 0; i < tickets.Count; i++)
        {
            Assert.Equal(tickets[i].Id, dtos[i].Id);
            Assert.Equal(tickets[i].PassengerName, dtos[i].PassengerName);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var tickets = new List<Ticket>();

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
