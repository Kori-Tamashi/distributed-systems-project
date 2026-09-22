using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.enums;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;
using Xunit;

using ServiceTicketNotFoundException = core.exceptions.businesslogic.services.TicketNotFoundException;
using ServiceTicketValidationException = core.exceptions.businesslogic.services.TicketValidationException;
using TicketDomain = core.domain.Ticket;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for TicketService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_tickets).
/// TicketService is tested with real TicketPostgresqlRepository.
/// 
/// TEST CONTEXT:
/// - TestPostgresDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - Each test method cleans up its own data in Dispose
/// - Tests share the same database but are isolated by cleanup
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - ticket exists in database
///    EP2: Non-existing ID (error case) - TicketNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - TicketValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single ticket (normal case) - should return list with one ticket
///    EP3: Multiple tickets (normal case) - should return all tickets
///    EP4: Filter by passenger email (normal case) - should return matching tickets
///    EP5: Filter by class (normal case) - should return matching tickets
///    EP6: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid ticket with all fields (normal case) - should create successfully
///    EP2: Valid ticket with minimal data (edge case) - should create successfully
///    EP3: Ticket with empty passenger name (validation case) - TicketValidationException thrown
///    EP4: Ticket with empty email (validation case) - TicketValidationException thrown
///    EP5: Ticket with future booking date (validation case) - TicketValidationException thrown
///    EP6: Ticket with negative price (validation case) - TicketValidationException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Non-existing ID (error case) - TicketNotFoundException thrown
///    EP3: Invalid passenger name (validation case) - TicketValidationException thrown
///    EP4: Invalid ID <= 0 (validation case) - TicketValidationException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - TicketNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - TicketValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - TicketValidationException thrown
/// 
/// 7. GetCountAsync Tests:
///    EP1: Empty database (edge case) - should return 0
///    EP2: Single ticket (normal case) - should return 1
///    EP3: Multiple tickets (normal case) - should return correct count
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// 
/// Total: 31 integration tests
/// </summary>
public class TicketServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresDatabaseContext _testContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketService _service;

    static TicketServiceIntegrationTests()
    {
        // Environment variables are read directly from Docker environment (no .env file needed)
        // TEST_POSTGRESQL_* variables are set in CI/CD workflow
    }

    public TicketServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresDatabaseContext();
        _testContext.EnsureDatabaseDeleted(); // Create tables automatically
        _testContext.EnsureDatabaseCreated();
        
        // Create repositories and service using the test database context
        _ticketRepository = new TicketPostgresqlRepository(_testContext);
        _service = new TicketService(_ticketRepository);
    }

    public void Dispose()
    {
        try
        {
            // Clean up all tickets
            var tickets = _testContext.Tickets.ToList();
            if (tickets.Any())
            {
                _testContext.Tickets.RemoveRange(tickets);
                _testContext.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _testContext.Dispose();
        }
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return ticket
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_TicketExists_ShouldReturnTicket()
    {
        // Arrange
        var ticket = new TicketBuilder().WithId(0).WithPassengerName("John Doe").WithPassengerEmail("john@example.com").Build();
        var created = await _service.CreateAsync(ticket);

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.PassengerName, result.PassengerName);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketNotFoundException>(
            () => _service.GetByIdAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.TicketId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: Empty database - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP2: Single ticket - should return list with one ticket
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleTicket_ShouldReturnOneTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        await _service.CreateAsync(ticket);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    /// <summary>
    /// EP3: Multiple tickets - should return all tickets
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleTickets_ShouldReturnAllTickets()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        foreach (var ticket in tickets)
        {
            ticket.Id = 0;
            await _service.CreateAsync(ticket);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by passenger email - should return matching tickets
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithEmailFilter_ShouldReturnMatchingTickets()
    {
        // Arrange
        var ticket1 = new TicketBuilder().WithId(0).WithPassengerEmail("test@example.com").Build();
        var ticket2 = new TicketBuilder().WithId(0).WithPassengerEmail("other@example.com").Build();
        await _service.CreateAsync(ticket1);
        await _service.CreateAsync(ticket2);

        // Act
        var result = await _service.GetAllAsync(new TicketFilter { PassengerEmail = "test@example.com" });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("test@example.com", result[0].PassengerEmail);
    }

    /// <summary>
    /// EP5: Filter by class - should return matching tickets
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithClassFilter_ShouldReturnMatchingTickets()
    {
        // Arrange
        var ticket1 = new TicketBuilder().WithId(0).WithEconomyClass().Build();
        var ticket2 = new TicketBuilder().WithId(0).WithBusinessClass().Build();
        await _service.CreateAsync(ticket1);
        await _service.CreateAsync(ticket2);

        // Act
        var result = await _service.GetAllAsync(new TicketFilter { Class = (int)TicketClass.Economy });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal((int)TicketClass.Economy, (int)result[0].Class);
    }

    /// <summary>
    /// EP6: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        await _service.CreateAsync(ticket);

        // Act
        var result = await _service.GetAllAsync(new TicketFilter { PassengerEmail = "nonexistent@example.com" });

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid ticket with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidTicket_ShouldCreateTicket()
    {
        // Arrange
        var ticket = new TicketBuilder().WithId(0).WithPassengerName("John Doe").WithPassengerEmail("john@example.com").Build();

        // Act
        var result = await _service.CreateAsync(ticket);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("John Doe", result.PassengerName);
    }

    /// <summary>
    /// EP2: Valid ticket with minimal data - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalTicket_ShouldCreateTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateMinimalTicket();
        ticket.Id = 0;

        // Act
        var result = await _service.CreateAsync(ticket);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
    }

    /// <summary>
    /// EP3: Ticket with empty passenger name - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyPassengerName_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        ticket.PassengerName = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Ticket with empty email - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyEmail_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        ticket.PassengerEmail = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Ticket with future booking date - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_FutureBookingDate_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        ticket.BookingDate = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP6: Ticket with negative price - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_NegativePrice_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        ticket.Price = -100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.CreateAsync(ticket)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdateTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        var created = await _service.CreateAsync(ticket);
        created.PassengerName = "Jane Doe";

        // Act
        var result = await _service.UpdateAsync(created);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.PassengerName);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketNotFoundException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Equal(ticket.Id, exception.TicketId);
    }

    /// <summary>
    /// EP3: Invalid passenger name - should throw TicketValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidPassengerName_ShouldThrowTicketValidationException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        var created = await _service.CreateAsync(ticket);
        created.PassengerName = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.UpdateAsync(created)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.UpdateAsync(ticket)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_ValidId_ShouldDeleteTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        var created = await _service.CreateAsync(ticket);

        // Act
        var result = await _service.DeleteAsync(created.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketNotFoundException>(
            () => _service.DeleteAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.TicketId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    /// <summary>
    /// EP4: Delete and verify data removed - GetById should throw
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        var created = await _service.CreateAsync(ticket);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert - Verify data is completely removed
        var exception = await Assert.ThrowsAsync<ServiceTicketNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.TicketId);
        
        // Also verify ExistsAsync returns false
        var exists = await _service.ExistsAsync(created.Id);
        Assert.False(exists);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_TicketExists_ShouldReturnTrue()
    {
        // Arrange
        var ticket = new TicketBuilder().WithId(0).WithPassengerName("John Doe").WithPassengerEmail("john@example.com").Build();
        var created = await _service.CreateAsync(ticket);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_TicketNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var ticket = new TicketBuilder().WithId(0).WithPassengerName("John Doe").WithPassengerEmail("john@example.com").Build();
        var created = await _service.CreateAsync(ticket);
        await _service.DeleteAsync(created.Id);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw TicketValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowTicketValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServiceTicketValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Ticket ID", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: Empty database - should return 0
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_EmptyDatabase_ShouldReturnZero()
    {
        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// EP2: Single ticket - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SingleTicket_ShouldReturnOne()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        ticket.Id = 0;
        await _service.CreateAsync(ticket);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// EP3: Multiple tickets - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultipleTickets_ShouldReturnCorrectCount()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(7);
        foreach (var ticket in tickets)
        {
            ticket.Id = 0;
            await _service.CreateAsync(ticket);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion
}
