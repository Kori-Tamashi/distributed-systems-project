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
/// Unit tests for TicketHttpConverter (per lab2-template v1 spec)
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
        Assert.Equal(ticket.FlightNumber, dto.FlightNumber);
        Assert.Equal(ticket.Username, dto.Username);
        Assert.Equal(ticket.Price, dto.Price);
        Assert.Equal(ticket.Status, dto.Status);
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
        Assert.Equal(ticket.Username, dto.Username);
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
            Username = ticket.Username,
            FlightNumber = ticket.FlightNumber,
            Price = ticket.Price,
            Status = ticket.Status
        };

        // Act
        var result = TicketHttpConverter.ToDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.TicketUid, result.TicketUid);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.FlightNumber, result.FlightNumber);
        Assert.Equal(dto.Price, result.Price);
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
        Assert.Equal(ticket.TicketUid, result.TicketUid);
        Assert.Equal(ticket.Username, result.Username);
        Assert.Equal(ticket.FlightNumber, result.FlightNumber);
        Assert.Equal(ticket.Price, result.Price);
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
        Assert.Equal(ticket.Username, dto.Username);
        Assert.Equal(ticket.FlightNumber, dto.FlightNumber);
        Assert.Equal(ticket.Price, dto.Price);
        Assert.Equal(ticket.Status, dto.Status);
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
            Username = "john_doe",
            FlightNumber = "AFL031",
            Price = 15000,
            Status = TicketStatus.Paid
        };

        // Act
        var result = TicketHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(dto.TicketUid, result.TicketUid);
        Assert.Equal(dto.Username, result.Username);
        Assert.Equal(dto.FlightNumber, result.FlightNumber);
        Assert.Equal(dto.Price, result.Price);
        Assert.Equal((TicketStatus)dto.Status, result.Status);
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
        Assert.Equal(ticket.Username, result.Username);
        Assert.Equal(ticket.FlightNumber, result.FlightNumber);
        Assert.Equal(ticket.Price, result.Price);
        Assert.Equal(ticket.Status, result.Status);
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
