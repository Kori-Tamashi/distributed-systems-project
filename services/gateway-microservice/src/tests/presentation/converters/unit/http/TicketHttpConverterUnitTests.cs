using presentation.converters.http;
using presentation.dto.http.Ticket;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation TicketHttpConverter
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
    [Fact]
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
    [Fact]
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

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToCreateDTO_ValidTicket_ShouldConvertCorrectly()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var createDto = TicketHttpConverter.ToCreateDTO(ticket);

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal(ticket.FlightId, createDto.FlightId);
        Assert.Equal(ticket.PassengerName, createDto.PassengerName);
        Assert.Equal(ticket.Price, createDto.Price);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToCreateDomain_CreateTicketDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalTicket = TicketMother.CreateValidTicket();
        var createDto = new CreateTicketDTO
        {
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
        var ticket = TicketHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalTicket.FlightId, ticket.FlightId);
        Assert.Equal(originalTicket.PassengerName, ticket.PassengerName);
        Assert.Equal(originalTicket.Price, ticket.Price);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToUpdateDTO_ValidTicket_ShouldConvertCorrectly()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();

        // Act
        var updateDto = TicketHttpConverter.ToUpdateDTO(ticket);

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(ticket.PassengerName, updateDto.PassengerName);
        Assert.Equal(ticket.Price, updateDto.Price);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdateTicketDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingTicket = TicketMother.CreateValidTicket();
        var updateDto = new UpdateTicketDTO
        {
            PassengerName = "Updated Name"
            // Other fields are null
        };

        // Act
        var updatedTicket = TicketHttpConverter.ToUpdateDomain(updateDto, existingTicket);

        // Assert
        Assert.Equal("Updated Name", updatedTicket.PassengerName);
        Assert.Equal(existingTicket.Price, updatedTicket.Price);
        Assert.Equal(existingTicket.SeatNumber, updatedTicket.SeatNumber);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of tickets
    /// </summary>
    [Fact]
    public void ToDTO_ListOfTickets_ShouldConvertAll()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(3);

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Count);
        for (int i = 0; i < tickets.Count; i++)
        {
            Assert.Equal(tickets[i].Id, dtos[i].Id);
            Assert.Equal(tickets[i].PassengerName, dtos[i].PassengerName);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    [Fact]
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var tickets = new List<core.domain.Ticket>();

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
