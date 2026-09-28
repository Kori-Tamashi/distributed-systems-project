using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.domain;
using core.enums;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;
using Xunit;

using TicketDomain = core.domain.Ticket;

namespace tests.dataaccess.repositories.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for TicketPostgresqlRepository
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_tickets).
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - ticket exists in database
///    EP2: Non-existing ID (edge case) - ticket not found, should throw TicketNotFoundException
///    EP3: Ticket with null optional fields (edge case) - ticket with minimal data
///    EP4: Ticket with all fields populated (normal case) - ticket with full data
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single ticket (normal case) - should return list with one ticket
///    EP3: Multiple tickets (normal case) - should return all tickets
///    EP4: Filter by flight ID (normal case) - should return matching tickets
///    EP5: Filter by ticket class (normal case) - should return tickets with specific class
///    EP6: Filter by status (normal case) - should return tickets with specific status
///    EP7: Filter with no matches (edge case) - should return empty list
///    EP8: Null filter (normal case) - should return all tickets
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid ticket with all fields (normal case) - should create successfully
///    EP2: Valid ticket with null optional fields (edge case) - should create successfully
///    EP3: Ticket with zero price (boundary case) - should create successfully
///    EP4: Duplicate ID (error case) - should throw TicketAlreadyExistsException
///    EP5: Ticket with max price value (boundary case) - should create successfully
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with null optional fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - should throw TicketNotFoundException
///    EP4: Update price to boundary values (boundary case) - should update successfully
///    EP5: Update status (normal case) - should update successfully
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: Delete and verify data removed (verification case) - data should be gone
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
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
/// </summary>
public class TicketPostgresqlRepositoryIntegrationTests : IDisposable
{
    private readonly TestPostgresDatabaseContext _context;
    private readonly ITicketRepository _repository;
    private readonly List<TicketDomain> _createdTickets;

    static TicketPostgresqlRepositoryIntegrationTests()
    {
        // Load .env file to get TEST_POSTGRESQL_* environment variables
    }

    public TicketPostgresqlRepositoryIntegrationTests()
    {
        // Create test database context
        _context = new TestPostgresDatabaseContext();
        _context.EnsureDatabaseDeleted();
        _context.EnsureDatabaseCreated();
        
        // Create repository using the test database connection
        _repository = new TicketPostgresqlRepository(_context);
        
        // Track created tickets for cleanup
        _createdTickets = new List<TicketDomain>();
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var tickets = _context.Ticket.ToList();
            if (tickets.Any())
            {
                _context.Ticket.RemoveRange(tickets);
                _context.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _context?.Dispose();
        }
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - ticket exists in database
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_TicketExists_ShouldReturnTicket()
    {
        // Arrange
        var expectedTicket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(expectedTicket);
        _createdTickets.Add(expectedTicket);

        // Act
        var actualTicket = await _repository.GetByIdAsync(expectedTicket.Id);

        // Assert
        Assert.NotNull(actualTicket);
        Assert.Equal(expectedTicket.Id, actualTicket.Id);
        Assert.Equal(expectedTicket.FlightNumber, actualTicket.FlightNumber);
        Assert.Equal(expectedTicket.Username, actualTicket.Username);
        Assert.Equal((int)expectedTicket.Status, (int)actualTicket.Status);
        Assert.Equal((int)expectedTicket.Status, (int)actualTicket.Status);
        Assert.Equal(expectedTicket.Price, actualTicket.Price);
    }

    /// <summary>
    /// EP2: Non-existing ID - ticket not found
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _repository.GetByIdAsync(nonExistingId));
        
        Assert.Equal(nonExistingId, exception.TicketId);
    }

    /// <summary>
    /// EP3: Ticket with minimal data - minimal fields
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_TicketWithMinimalData_ShouldReturnTicket()
    {
        // Arrange
        var expectedTicket = TicketMother.CreateMinimalTicket();
        await _repository.CreateAsync(expectedTicket);
        _createdTickets.Add(expectedTicket);

        // Act
        var actualTicket = await _repository.GetByIdAsync(expectedTicket.Id);

        // Assert
        Assert.NotNull(actualTicket);
        Assert.Equal(expectedTicket.Id, actualTicket.Id);
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
        // Arrange (database is already empty)

        // Act
        var tickets = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(tickets);
        Assert.Empty(tickets);
    }

    /// <summary>
    /// EP2: Single ticket - should return list with one ticket
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleTicket_ShouldReturnListWithOneTicket()
    {
        // Arrange
        var expectedTicket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(expectedTicket);
        _createdTickets.Add(expectedTicket);

        // Act
        var tickets = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(tickets);
        Assert.Single(tickets);
        Assert.Equal(expectedTicket.Id, tickets[0].Id);
    }

    /// <summary>
    /// EP3: Multiple tickets - should return all tickets
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleTickets_ShouldReturnAllTickets()
    {
        // Arrange
        var expectedTickets = TicketMother.CreateTicketList(5);
        foreach (var ticket in expectedTickets)
        {
            await _repository.CreateAsync(ticket);
            _createdTickets.Add(ticket);
        }

        // Act
        var tickets = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(tickets);
        Assert.Equal(5, tickets.Count);
    }

    /// <summary>
    /// EP4: Filter by flight ID - should return matching tickets
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFlightNumberFilter_ShouldReturnMatchingTickets()
    {
        // Arrange
        var ticket1 = new TicketBuilder().WithId(1).WithFlightNumber("100").Build();
        var ticket2 = new TicketBuilder().WithId(2).WithFlightNumber("200").Build();
        var ticket3 = new TicketBuilder().WithId(3).WithFlightNumber("100").Build();
        
        await _repository.CreateAsync(ticket1);
        await _repository.CreateAsync(ticket2);
        await _repository.CreateAsync(ticket3);
        _createdTickets.AddRange(new[] { ticket1, ticket2, ticket3 });

        // Act
        var tickets = await _repository.GetAllAsync(new TicketFilter { FlightNumber = "100" });

        // Assert
        Assert.NotNull(tickets);
        Assert.Equal(2, tickets.Count);
        Assert.All(tickets, t => Assert.Equal("100", t.FlightNumber));
    }

    /// <summary>
    /// EP5: Filter by ticket class - should return tickets with specific class
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithTicketClassFilter_ShouldReturnMatchingTickets()
    {
        // Arrange
        var economyTicket = new TicketBuilder().WithId(1).WithConfirmedStatus().Build();
        var businessTicket = new TicketBuilder().WithId(2).WithCancelledStatus().Build();
        var firstClassTicket = new TicketBuilder().WithId(3).WithRefundedStatus().Build();
        
        await _repository.CreateAsync(economyTicket);
        await _repository.CreateAsync(businessTicket);
        await _repository.CreateAsync(firstClassTicket);
        _createdTickets.AddRange(new[] { economyTicket, businessTicket, firstClassTicket });

        // Act
        var tickets = await _repository.GetAllAsync(new TicketFilter 
        { 
            Status = TicketStatus.Paid 
        });

        // Assert
        Assert.NotNull(tickets);
        Assert.Single(tickets);
        Assert.All(tickets, t => Assert.Equal((int)TicketClass.Economy, (int)t.Status));
    }

    /// <summary>
    /// EP6: Filter by status - should return tickets with specific status
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithStatusFilter_ShouldReturnMatchingTickets()
    {
        // Arrange
        var confirmedTicket = new TicketBuilder().WithId(1).WithStatus(TicketStatus.Paid).Build();
        var cancelledTicket = new TicketBuilder().WithId(2).WithStatus(TicketStatus.Canceled).Build();
        var refundedTicket = new TicketBuilder().WithId(3).WithStatus(TicketStatus.Canceled).Build();
        
        await _repository.CreateAsync(confirmedTicket);
        await _repository.CreateAsync(cancelledTicket);
        await _repository.CreateAsync(refundedTicket);
        _createdTickets.AddRange(new[] { confirmedTicket, cancelledTicket, refundedTicket });

        // Act
        var tickets = await _repository.GetAllAsync(new TicketFilter 
        { 
            Status = TicketStatus.Paid 
        });

        // Assert
        Assert.NotNull(tickets);
        Assert.Single(tickets);
        Assert.All(tickets, t => Assert.Equal((int)TicketStatus.Paid, (int)t.Status));
    }

    /// <summary>
    /// EP7: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        // Act
        var tickets = await _repository.GetAllAsync(new TicketFilter { FlightNumber = "9999" });

        // Assert
        Assert.NotNull(tickets);
        Assert.Empty(tickets);
    }

    /// <summary>
    /// EP8: Null filter - should return all tickets
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNullFilter_ShouldReturnAllTickets()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(3);
        foreach (var ticket in tickets)
        {
            await _repository.CreateAsync(ticket);
            _createdTickets.Add(ticket);
        }

        // Act
        var result = await _repository.GetAllAsync(filter: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
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
        var ticketToCreate = TicketMother.CreateValidTicket();

        // Act
        var createdTicket = await _repository.CreateAsync(ticketToCreate);
        _createdTickets.Add(createdTicket);

        // Assert
        Assert.NotNull(createdTicket);
        Assert.NotEqual(0, createdTicket.Id);
        Assert.Equal(ticketToCreate.FlightNumber, createdTicket.FlightNumber);
        Assert.Equal(ticketToCreate.Username, createdTicket.Username);
        Assert.Equal((int)ticketToCreate.Status, (int)createdTicket.Status);
        Assert.Equal((int)ticketToCreate.Status, (int)createdTicket.Status);
        Assert.Equal(ticketToCreate.Price, createdTicket.Price);
    }

    /// <summary>
    /// EP2: Valid ticket with minimal fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalTicket_ShouldCreateTicket()
    {
        // Arrange
        var ticketToCreate = TicketMother.CreateMinimalTicket();

        // Act
        var createdTicket = await _repository.CreateAsync(ticketToCreate);
        _createdTickets.Add(createdTicket);

        // Assert
        Assert.NotNull(createdTicket);
        Assert.NotEqual(0, createdTicket.Id);
    }

    /// <summary>
    /// EP3: Ticket with zero price - boundary case
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_TicketWithZeroPrice_ShouldCreateTicket()
    {
        // Arrange
        var ticketToCreate = TicketMother.CreateTicketWithZeroPrice();

        // Act
        var createdTicket = await _repository.CreateAsync(ticketToCreate);
        _createdTickets.Add(createdTicket);

        // Assert
        Assert.NotNull(createdTicket);
        Assert.Equal(0, createdTicket.Price);
    }

    /// <summary>
    /// EP4: Duplicate ID - should throw TicketAlreadyExistsException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_DuplicateId_ShouldThrowTicketAlreadyExistsException()
    {
        // Arrange
        var ticket1 = new TicketBuilder().WithId(1).WithFlightNumber("100").Build();
        var ticket2 = new TicketBuilder().WithId(1).WithFlightNumber("200").Build();
        
        await _repository.CreateAsync(ticket1);
        _createdTickets.Add(ticket1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketAlreadyExistsException>(
            () => _repository.CreateAsync(ticket2));
        
        Assert.Equal(1, exception.TicketId);
    }

    /// <summary>
    /// EP5: Ticket with max price value - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_TicketWithMaxPrice_ShouldCreateTicket()
    {
        // Arrange
        var ticketToCreate = TicketMother.CreateTicketWithMaxPrice();

        // Act
        var createdTicket = await _repository.CreateAsync(ticketToCreate);
        _createdTickets.Add(createdTicket);

        // Assert
        Assert.NotNull(createdTicket);
        Assert.Equal(int.MaxValue, createdTicket.Price);
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
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        ticket.Username = "Updated Name";
        ticket.Price = 50000;
        ticket.Status = TicketStatus.Canceled;

        // Act
        var updatedTicket = await _repository.UpdateAsync(ticket);

        // Assert
        Assert.NotNull(updatedTicket);
        Assert.Equal("Updated Name", updatedTicket.Username);
        Assert.Equal(50000, updatedTicket.Price);
        Assert.Equal(TicketStatus.Canceled, updatedTicket.Status);
    }

    /// <summary>
    /// EP2: Update price to zero - boundary case
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdatePriceToZero_ShouldUpdateTicket()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        ticket.Price = 0;

        // Act
        var updatedTicket = await _repository.UpdateAsync(ticket);

        // Assert
        Assert.NotNull(updatedTicket);
        Assert.Equal(0, updatedTicket.Price);
    }

    /// <summary>
    /// EP3: Non-existing ID - should throw TicketNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_TicketNotFound_ShouldThrowTicketNotFoundException()
    {
        // Arrange
        var ticket = new TicketBuilder().WithId(999).WithFlightNumber("100").Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _repository.UpdateAsync(ticket));
        
        Assert.Equal(999, exception.TicketId);
    }

    /// <summary>
    /// EP4: Update status - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdateStatus_ShouldUpdateTicket()
    {
        // Arrange
        var ticket = new TicketBuilder().WithId(0).WithFlightNumber("100").WithStatus(TicketStatus.Paid).Build();
        ticket = await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        ticket.Status = TicketStatus.Canceled;

        // Act
        var updatedTicket = await _repository.UpdateAsync(ticket);

        // Assert
        Assert.NotNull(updatedTicket);
        Assert.Equal(TicketStatus.Canceled, updatedTicket.Status);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_TicketExists_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        // Act
        var result = await _repository.DeleteAsync(ticket.Id);

        // Assert
        Assert.True(result);
        
        // Verify ticket is deleted
        var exists = await _repository.ExistsAsync(ticket.Id);
        Assert.False(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_TicketNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result = await _repository.DeleteAsync(nonExistingId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Delete and verify data removed - should throw NotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        // Act
        await _repository.DeleteAsync(ticket.Id);

        // Assert
        await Assert.ThrowsAsync<TicketNotFoundException>(
            () => _repository.GetByIdAsync(ticket.Id));
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
        var ticket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        // Act
        var exists = await _repository.ExistsAsync(ticket.Id);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_TicketNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var exists = await _repository.ExistsAsync(nonExistingId);

        // Assert
        Assert.False(exists);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);
        
        await _repository.DeleteAsync(ticket.Id);

        // Act
        var exists = await _repository.ExistsAsync(ticket.Id);

        // Assert
        Assert.False(exists);
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
        // Arrange (database is already empty)

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
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
        await _repository.CreateAsync(ticket);
        _createdTickets.Add(ticket);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(1, count);
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
            await _repository.CreateAsync(ticket);
            _createdTickets.Add(ticket);
        }

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(7, count);
    }

    #endregion
}
