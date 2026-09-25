using presentation.converters.http;
using presentation.dto.http.Ticket;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation TicketHttpConverter (per lab2-template v1 spec)
/// AAA Structure: Arrange - Act - Assert
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
        Assert.Equal(ticket.Username, dto.Username);
        Assert.Equal(ticket.FlightNumber, dto.FlightNumber);
        Assert.Equal(ticket.Price, dto.Price);
        Assert.Equal(ticket.Status, dto.Status);
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
        Assert.Equal(ticket.Username, dto.Username);
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
        Assert.Equal(originalTicket.Username, convertedTicket.Username);
        Assert.Equal(originalTicket.FlightNumber, convertedTicket.FlightNumber);
        Assert.Equal(originalTicket.Price, convertedTicket.Price);
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
        Assert.Equal(ticket.Username, createDto.Username);
        Assert.Equal(ticket.FlightNumber, createDto.FlightNumber);
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
        var createDto = new CreateTicketDTO(
            ticketUid: originalTicket.TicketUid,
            username: originalTicket.Username,
            flightNumber: originalTicket.FlightNumber,
            price: originalTicket.Price,
            status: originalTicket.Status
        );

        // Act
        var ticket = TicketHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalTicket.Username, ticket.Username);
        Assert.Equal(originalTicket.FlightNumber, ticket.FlightNumber);
        Assert.Equal(originalTicket.Price, ticket.Price);
        Assert.Equal(originalTicket.Status, ticket.Status);
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
        Assert.Equal(ticket.Username, updateDto.Username);
        Assert.Equal(ticket.FlightNumber, updateDto.FlightNumber);
        Assert.Equal(ticket.Price, updateDto.Price);
        Assert.Equal(ticket.Status, updateDto.Status);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdateTicketDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingTicket = TicketMother.CreateValidTicket();
        var updateDto = new UpdateTicketDTO(
            ticketUid: Guid.NewGuid(),
            username: "Updated Name"
            // Other fields are null
        );

        // Act
        var updatedTicket = TicketHttpConverter.ToUpdateDomain(updateDto, existingTicket);

        // Assert
        Assert.Equal("Updated Name", updatedTicket.Username);
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
            Assert.Equal(tickets[i].Username, dtos[i].Username);
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
