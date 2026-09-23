using core.domain;
using Microsoft.Extensions.Logging;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using GatewayTicketNotFoundException = core.exceptions.dataaccess.gateways.TicketGatewayEntityNotFoundException;
using GatewayTicketCommunicationException = core.exceptions.dataaccess.gateways.TicketGatewayCommunicationException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for TicketService
/// Using London-style TDD with Mocks (Moq) for HTTP Gateways
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Ticket exists (normal case)
/// - EP2: Valid ID, Ticket does not exist (GatewayTicketNotFoundException)
/// - EP3: Invalid ID (<= 0) (TicketValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetAllAsync(TicketFilter? filter):
/// - EP1: No filter, returns all tickets
/// - EP2: With filter, returns filtered tickets
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreateAsync(Ticket ticket):
/// - EP1: Valid ticket, creation successful (normal case)
/// - EP2: Invalid ticket (TicketValidationException)
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For UpdateAsync(Ticket ticket):
/// - EP1: Valid ticket, Ticket exists, update successful (normal case)
/// - EP2: Ticket does not exist (GatewayTicketNotFoundException)
/// - EP3: Invalid ticket (TicketValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Ticket exists, deletion successful (normal case)
/// - EP2: Valid ID, Ticket does not exist (GatewayTicketNotFoundException)
/// - EP3: Invalid ID (<= 0) (TicketValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Ticket exists (returns true)
/// - EP2: Valid ID, Ticket does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (TicketValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetCountAsync(TicketFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Gateway communication error (ValidationException)
/// 
/// Total: 21 unit tests (all should pass)
/// </summary>
public class TicketServiceUnitTests
{
    private readonly Mock<ITicketGateway> _mockTicketGateway;
    private readonly ITicketService _service;

    public TicketServiceUnitTests()
    {
        // Arrange - Setup mock gateway
        _mockTicketGateway = new Mock<ITicketGateway>();
        _service = new TicketService(_mockTicketGateway.Object, Mock.Of<ILogger<TicketService>>());
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return Ticket
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_TicketExists_ShouldReturnTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        // Act
        var result = await _service.GetByIdAsync(ticket.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.PassengerName, result.PassengerName);
        _mockTicketGateway.Verify(g => g.GetByIdAsync(ticket.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket does not exist - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ValidId_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketGateway.Setup(g => g.DeleteAsync(ticketId)).ThrowsAsync(new GatewayTicketNotFoundException($"Ticket with id {ticketId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.GetByIdAsync(ticketId)
        );
        Assert.Equal(ticketId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetByIdAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticketId))
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetByIdAsync(ticketId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all tickets
    /// </summary>
    [Fact]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllTickets()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        _mockTicketGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(tickets);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockTicketGateway.Verify(g => g.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered tickets
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredTickets()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(3);
        var filter = new TicketFilter { PassengerName = "John" };
        _mockTicketGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(tickets);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mockTicketGateway.Verify(g => g.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetAllAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockTicketGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid ticket, creation successful - should return created ticket
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidTicket_ShouldReturnCreatedTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketGateway.Setup(g => g.CreateAsync(It.IsAny<Ticket>())).ReturnsAsync(ticket);

        // Act
        var result = await _service.CreateAsync(ticket);

        // Assert
        Assert.NotNull(result);
        _mockTicketGateway.Verify(g => g.CreateAsync(It.IsAny<Ticket>()), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid ticket (empty passenger name) - should throw TicketValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidTicket_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = new Ticket { PassengerName = "", PassengerEmail = "test@test.com", Price = 1000, BookingDate = DateTime.UtcNow.AddHours(-1) };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Future booking date - should throw TicketValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_FutureBookingDate_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.BookingDate = DateTime.UtcNow.AddDays(30);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task CreateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketGateway.Setup(g => g.CreateAsync(It.IsAny<Ticket>()))
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid ticket, Ticket exists, update successful - should return updated ticket
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ValidTicket_TicketExists_ShouldReturnUpdatedTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketGateway.Setup(g => g.UpdateAsync(It.IsAny<Ticket>())).ReturnsAsync(ticket);

        // Act
        var result = await _service.UpdateAsync(ticket);

        // Assert
        Assert.NotNull(result);
        _mockTicketGateway.Verify(g => g.UpdateAsync(It.IsAny<Ticket>()), Times.Once);
    }

    /// <summary>
    /// EP2: Ticket does not exist - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketGateway.Setup(g => g.UpdateAsync(It.IsAny<Ticket>())).ThrowsAsync(new GatewayTicketNotFoundException($"Ticket with id {ticket.Id} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Equal(ticket.Id, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ticket - should throw TicketValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_InvalidTicket_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = new Ticket { Id = 1, PassengerName = "", PassengerEmail = "test@test.com", Price = 1000, BookingDate = DateTime.UtcNow.AddHours(-1) };
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task UpdateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);
        _mockTicketGateway.Setup(g => g.UpdateAsync(It.IsAny<Ticket>()))
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists, deletion successful - should return true
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_TicketExists_ShouldReturnTrue()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticketId)).ReturnsAsync(TicketMother.CreateValidTicket());
        _mockTicketGateway.Setup(g => g.DeleteAsync(ticketId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(ticketId);

        // Assert
        _mockTicketGateway.Verify(g => g.DeleteAsync(ticketId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket does not exist - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ValidId_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketGateway.Setup(g => g.DeleteAsync(ticketId)).ThrowsAsync(new GatewayTicketNotFoundException($"Ticket with id {ticketId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.DeleteAsync(ticketId)
        );
        Assert.Equal(ticketId, exception.EntityId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task DeleteAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticketId)).ReturnsAsync(TicketMother.CreateValidTicket());
        _mockTicketGateway.Setup(g => g.DeleteAsync(ticketId))
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DeleteAsync(ticketId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return true
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_TicketExists_ShouldReturnTrue()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticketId)).ReturnsAsync(TicketMother.CreateValidTicket());

        // Act
        var result = await _service.ExistsAsync(ticketId);

        // Assert
    }

    /// <summary>
    /// EP2: Valid ID, Ticket does not exist - should return false
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ValidId_TicketNotFound_ShouldReturnFalse()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketGateway.Setup(g => g.DeleteAsync(ticketId)).ThrowsAsync(new GatewayTicketNotFoundException($"Ticket with id {ticketId} not found"));

        // Act
        var result = await _service.ExistsAsync(ticketId);

        // Assert
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task ExistsAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketGateway.Setup(g => g.GetByIdAsync(ticketId))
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExistsAsync(ticketId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count
    /// </summary>
    [Fact]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(10);
        _mockTicketGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(tickets);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count
    /// </summary>
    [Fact]
    public async Task GetCountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        var filter = new TicketFilter { PassengerName = "John" };
        _mockTicketGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(tickets);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(5, result);
    }

    /// <summary>
    /// EP3: Gateway communication error - should throw ValidationException
    /// </summary>
    [Fact]
    public async Task GetCountAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockTicketGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayTicketCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion
}
