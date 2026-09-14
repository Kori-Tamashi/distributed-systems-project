using core.domain;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using RepositoryTicketAlreadyExistsException = core.exceptions.dataaccess.repositories.TicketAlreadyExistsException;
using RepositoryTicketNotFoundException = core.exceptions.dataaccess.repositories.TicketNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for TicketService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Ticket exists (normal case)
/// - EP2: Valid ID, Ticket does not exist (TicketNotFoundException)
/// - EP3: Invalid ID (<= 0) (TicketValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(TicketFilter? filter):
/// - EP1: No filter, returns all tickets
/// - EP2: With filter, returns filtered tickets
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(Ticket ticket):
/// - EP1: Valid ticket, creation successful (normal case)
/// - EP2: Invalid ticket (TicketValidationException)
/// - EP3: Ticket already exists (TicketBusinessRuleViolationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(Ticket ticket):
/// - EP1: Valid ticket, Ticket exists, update successful (normal case)
/// - EP2: Ticket does not exist (TicketNotFoundException)
/// - EP3: Invalid ticket (TicketValidationException)
/// - EP4: Invalid ID (TicketValidationException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Ticket exists, deletion successful (normal case)
/// - EP2: Valid ID, Ticket does not exist (TicketNotFoundException)
/// - EP3: Invalid ID (<= 0) (TicketValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Ticket exists (returns true)
/// - EP2: Valid ID, Ticket does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (TicketValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync(TicketFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// Total: 27 unit tests (all should pass)
/// </summary>
public class TicketServiceUnitTests
{
    private readonly Mock<ITicketRepository> _mockTicketRepository;
    private readonly ITicketService _service;

    public TicketServiceUnitTests()
    {
        // Arrange - Setup mock repositories
        _mockTicketRepository = new Mock<ITicketRepository>();
        _service = new TicketService(_mockTicketRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return Ticket
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_TicketExists_ShouldReturnTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

        // Act
        var result = await _service.GetByIdAsync(ticket.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.PassengerName, result.PassengerName);
        _mockTicketRepository.Verify(r => r.GetByIdAsync(ticket.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket does not exist - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId)).ThrowsAsync(new RepositoryTicketNotFoundException(ticketId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.GetByIdAsync(ticketId)
        );
        Assert.Equal(ticketId, exception.TicketId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(ticketId)
        );
        Assert.Contains($"Failed to get Ticket with ID {ticketId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all tickets - should return list of tickets
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllTickets()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        _mockTicketRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(tickets);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockTicketRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered tickets - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.TicketFilter { PassengerEmail = "test@example.com" };
        var filteredTickets = new List<Ticket> { TicketMother.CreateValidTicket() };
        _mockTicketRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredTickets);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockTicketRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockTicketRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all Tickets", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid ticket, creation successful - should return created ticket
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidTicket_ShouldCreateTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.CreateAsync(ticket)).ReturnsAsync(ticket);

        // Act
        var result = await _service.CreateAsync(ticket);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.PassengerName, result.PassengerName);
        _mockTicketRepository.Verify(r => r.CreateAsync(ticket), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid ticket (null) - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_NullTicket_ShouldThrowTicketValidationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.CreateAsync(null!)
        );
        Assert.Contains("Ticket cannot be null", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid ticket (invalid email) - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidEmail_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.PassengerEmail = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Ticket already exists - should throw TicketBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_TicketAlreadyExists_ShouldThrowTicketBusinessRuleViolationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.CreateAsync(ticket))
            .ThrowsAsync(new RepositoryTicketAlreadyExistsException(ticket.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketBusinessRuleViolationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("Ticket", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.CreateAsync(ticket))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("Failed to create Ticket", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid ticket, Ticket exists, update successful - should return updated ticket
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidTicket_TicketExists_ShouldUpdateTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticket.Id)).ReturnsAsync(true);
        _mockTicketRepository.Setup(r => r.UpdateAsync(ticket)).ReturnsAsync(ticket);

        // Act
        var result = await _service.UpdateAsync(ticket);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        _mockTicketRepository.Verify(r => r.ExistsAsync(ticket.Id), Times.Once);
        _mockTicketRepository.Verify(r => r.UpdateAsync(ticket), Times.Once);
    }

    /// <summary>
    /// EP2: Ticket does not exist - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticket.Id)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Equal(ticket.Id, exception.TicketId);
    }

    /// <summary>
    /// EP3: Invalid ticket (invalid price) - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_InvalidPrice_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Price = -100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticket.Id)).ReturnsAsync(true);
        _mockTicketRepository.Setup(r => r.UpdateAsync(ticket))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Contains($"Failed to update Ticket with ID {ticket.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_TicketExists_ShouldDeleteTicket()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticketId)).ReturnsAsync(true);
        _mockTicketRepository.Setup(r => r.DeleteAsync(ticketId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(ticketId);

        // Assert
        Assert.True(result);
        _mockTicketRepository.Verify(r => r.ExistsAsync(ticketId), Times.Once);
        _mockTicketRepository.Verify(r => r.DeleteAsync(ticketId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket does not exist - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticketId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _service.DeleteAsync(ticketId)
        );
        Assert.Equal(ticketId, exception.TicketId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticketId)).ReturnsAsync(true);
        _mockTicketRepository.Setup(r => r.DeleteAsync(ticketId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(ticketId)
        );
        Assert.Contains($"Failed to delete Ticket with ID {ticketId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Ticket exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_TicketExists_ShouldReturnTrue()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticketId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(ticketId);

        // Assert
        Assert.True(result);
        _mockTicketRepository.Verify(r => r.ExistsAsync(ticketId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Ticket does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_TicketNotFound_ShouldReturnFalse()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticketId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(ticketId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var ticketId = 1;
        _mockTicketRepository.Setup(r => r.ExistsAsync(ticketId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(ticketId)
        );
        Assert.Contains($"Failed to check existence of Ticket with ID {ticketId}", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count - should return count from repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 42;
        _mockTicketRepository.Setup(r => r.GetCountAsync(null)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockTicketRepository.Verify(r => r.GetCountAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.TicketFilter { MinPrice = 1000, MaxPrice = 5000 };
        var expectedCount = 5;
        _mockTicketRepository.Setup(r => r.GetCountAsync(filter)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockTicketRepository.Verify(r => r.GetCountAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockTicketRepository.Setup(r => r.GetCountAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get Ticket count", exception.Message);
    }

    #endregion
}
