using System.Linq.Expressions;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using dataaccess.repositories.postgres;
using Microsoft.EntityFrameworkCore;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using TicketDomain = core.domain.Ticket;
using TicketPostgresqlModel = dataaccess.models.postgres.TicketPostgresqlModel;

namespace tests.dataaccess.repositories.unit.postgres;

/// <summary>
/// Unit tests for TicketPostgresqlRepository
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// </summary>
public class TicketPostgresqlRepositoryUnitTests
{
    private readonly Mock<TicketsDatabaseContext> _mockContext;
    private readonly Mock<DbSet<TicketPostgresqlModel>> _mockDbSet;
    private readonly TicketPostgresqlRepository _repository;

    public TicketPostgresqlRepositoryUnitTests()
    {
        // Arrange - Setup mock context
        _mockContext = new Mock<TicketsDatabaseContext>();
        _mockDbSet = new Mock<DbSet<TicketPostgresqlModel>>();
        
        // Setup DbSet to behave like IQueryable
        var data = new List<TicketPostgresqlModel>().AsQueryable();
        _mockDbSet.As<IQueryable<TicketPostgresqlModel>>()
            .Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<TicketPostgresqlModel>>()
            .Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<TicketPostgresqlModel>>()
            .Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<TicketPostgresqlModel>>()
            .Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        _mockContext.Setup(c => c.Tickets).Returns(_mockDbSet.Object);
        _repository = new TicketPostgresqlRepository(_mockContext.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_TicketExists_ShouldReturnTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        var model = new TicketPostgresqlModel
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

        _mockDbSet.Setup(m => m.FindAsync(ticket.Id)).ReturnsAsync(model);

        // Act
        var result = await _repository.GetByIdAsync(ticket.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.PassengerName, result.PassengerName);
        _mockDbSet.Verify(m => m.FindAsync(ticket.Id), Times.Once);
    }

    [Fact]
    [Unit]
    public async Task GetByIdAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticketId = 999;
        _mockDbSet.Setup(m => m.FindAsync(ticketId)).ReturnsAsync((TicketPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _repository.GetByIdAsync(ticketId)
        );
        Assert.Equal(ticketId, exception.TicketId);
    }

    [Fact]
    [Unit]
    public async Task GetByIdAsync_DatabaseError_ShouldThrowTicketDatabaseException()
    {
        // Arrange
        var ticketId = 1;
        _mockDbSet.Setup(m => m.FindAsync(ticketId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketDatabaseException>(
            () => _repository.GetByIdAsync(ticketId)
        );
        Assert.Contains("Failed to get Ticket by id", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Unit]
    public async Task CreateAsync_DatabaseError_ShouldThrowTicketDatabaseException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketDatabaseException>(
            () => _repository.CreateAsync(ticket)
        );
        Assert.Contains("Failed to create Ticket", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    [Unit]
    public async Task UpdateAsync_Success_ShouldReturnUpdatedTicket()
    {
        // Arrange
        var existingModel = new TicketPostgresqlModel
        {
            Id = 1,
            TicketUid = Guid.NewGuid(),
            FlightId = 1,
            PassengerName = "OLD NAME",
            PassengerEmail = "old@example.com",
            PassengerPhone = "+70000000000",
            SeatNumber = "10A",
            Class = (int)TicketClass.Economy,
            Price = 10000,
            BookingDate = DateTime.Now,
            Status = (int)TicketStatus.Confirmed
        };

        var updatedTicket = TicketMother.CreateValidTicket();
        updatedTicket.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedTicket);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedTicket.PassengerName, result.PassengerName);
        Assert.Equal(updatedTicket.Price, result.Price);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Unit]
    public async Task UpdateAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        _mockDbSet.Setup(m => m.FindAsync(ticket.Id)).ReturnsAsync((TicketPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _repository.UpdateAsync(ticket)
        );
        Assert.Equal(ticket.Id, exception.TicketId);
    }

    [Fact]
    [Unit]
    public async Task UpdateAsync_DatabaseError_ShouldThrowTicketDatabaseException()
    {
        // Arrange
        var existingModel = new TicketPostgresqlModel { Id = 1, PassengerName = "OLD NAME" };
        var updatedTicket = TicketMother.CreateValidTicket();
        updatedTicket.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketDatabaseException>(
            () => _repository.UpdateAsync(updatedTicket)
        );
        Assert.Contains("Failed to update Ticket", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    [Unit]
    public async Task DeleteAsync_Success_ShouldReturnTrue()
    {
        // Arrange
        var ticketId = 1;
        var existingModel = new TicketPostgresqlModel { Id = ticketId, PassengerName = "John Doe" };

        _mockDbSet.Setup(m => m.FindAsync(ticketId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(ticketId);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Unit]
    public async Task DeleteAsync_TicketNotFound_ShouldReturnFalse()
    {
        // Arrange
        var ticketId = 999;
        _mockDbSet.Setup(m => m.FindAsync(ticketId)).ReturnsAsync((TicketPostgresqlModel?)null);

        // Act
        var result = await _repository.DeleteAsync(ticketId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    [Unit]
    public async Task DeleteAsync_DatabaseError_ShouldThrowTicketDatabaseException()
    {
        // Arrange
        var ticketId = 1;
        var existingModel = new TicketPostgresqlModel { Id = ticketId, PassengerName = "John Doe" };

        _mockDbSet.Setup(m => m.FindAsync(ticketId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketDatabaseException>(
            () => _repository.DeleteAsync(ticketId)
        );
        Assert.Contains("Failed to delete Ticket", exception.Message);
    }

    #endregion
}
