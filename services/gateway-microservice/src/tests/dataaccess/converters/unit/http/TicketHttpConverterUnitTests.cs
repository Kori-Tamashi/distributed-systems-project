using core.domain;
using dataaccess.converters.http;
using dataaccess.dto.http.Ticket;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for TicketHttpConverter (per lab2-template v1 spec)
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

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_CreateTicketDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreateTicketDTO(
            ticketUid: Guid.NewGuid(),
            username: "test_user",
            flightNumber: "AFL031",
            price: 15000,
            status: 1
        );

        // Act
        var ticket = TicketHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.TicketUid, ticket.TicketUid);
        Assert.Equal(createDto.Username, ticket.Username);
        Assert.Equal(createDto.FlightNumber, ticket.FlightNumber);
        Assert.Equal(createDto.Price, ticket.Price);
        Assert.Equal(createDto.Status, ticket.Status);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToDomain_CreateTicketDTO_RoundTripShouldPreserveData()
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
        var ticket = TicketHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalTicket.TicketUid, ticket.TicketUid);
        Assert.Equal(originalTicket.Username, ticket.Username);
        Assert.Equal(originalTicket.FlightNumber, ticket.FlightNumber);
        Assert.Equal(originalTicket.Price, ticket.Price);
        Assert.Equal(originalTicket.Status, ticket.Status);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdateTicketDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdateTicketDTO(
            ticketUid: Guid.NewGuid(),
            username: "updated_user",
            flightNumber: "AFL032",
            price: 20000,
            status: 2
        );

        // Act
        var ticket = TicketHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal("updated_user", ticket.Username);
        Assert.Equal("AFL032", ticket.FlightNumber);
        Assert.Equal(20000, ticket.Price);
        Assert.Equal(2, ticket.Status);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdateTicketDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdateTicketDTO(
            ticketUid: Guid.NewGuid()
        );

        // Act
        var ticket = TicketHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(string.Empty, ticket.Username);
        Assert.Equal(string.Empty, ticket.FlightNumber);
        Assert.Equal(0, ticket.Price);
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
        var tickets = TicketMother.CreateTicketList(5);

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
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
        var tickets = new List<Ticket>();

        // Act
        var dtos = TicketHttpConverter.ToDTO(tickets).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
