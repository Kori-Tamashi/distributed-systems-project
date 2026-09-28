using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using core.enums;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;
using Xunit;

using TicketDomain = core.domain.Ticket;
using TicketPostgresqlModel = dataaccess.models.postgres.TicketPostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for TicketPostgresqlConverter (per lab2-template v1 spec)
/// Table: ticket
/// Columns: ticket_uid, username, flight_number, price, status
/// </summary>
public class TicketPostgresqlConverterUnitTests
{
    #region ToDomain Tests

    [Fact]
    [Unit]
    public void ToDomain_WithValidFullModel_ShouldReturnDomainWithAllProperties()
    {
        // Arrange
        var ticket = TicketMother.CreateValidTicket();
        var modelToConvert = new TicketPostgresqlModel
        {
            Id = ticket.Id,
            TicketUid = ticket.TicketUid,
            Username = ticket.Username,
            FlightNumber = ticket.FlightNumber,
            Price = ticket.Price,
            Status = ticket.Status
        };

        // Act
        var result = TicketPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.TicketUid, result.TicketUid);
        Assert.Equal(modelToConvert.Username, result.Username);
        Assert.Equal(modelToConvert.FlightNumber, result.FlightNumber);
        Assert.Equal(modelToConvert.Price, result.Price);
        Assert.Equal(modelToConvert.Status, result.Status);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMinValues_ShouldReturnDomainWithMinValues()
    {
        // Arrange
        var modelToConvert = new TicketPostgresqlModel
        {
            Id = 1,
            TicketUid = Guid.Empty,
            Username = "A",
            FlightNumber = "AFL001",
            Price = 0,
            Status = TicketStatus.Paid
        };

        // Act
        var result = TicketPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.TicketUid);
        Assert.Equal("A", result.Username);
        Assert.Equal("AFL001", result.FlightNumber);
        Assert.Equal(0, result.Price);
        Assert.Equal(TicketStatus.Paid, result.Status);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMaxValues_ShouldReturnDomainWithMaxValues()
    {
        // Arrange
        var modelToConvert = new TicketPostgresqlModel
        {
            Id = int.MaxValue,
            TicketUid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
            Username = new string('A', 80),
            FlightNumber = "AFL999",
            Price = int.MaxValue,
            Status = TicketStatus.Canceled
        };

        // Act
        var result = TicketPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Id);
        Assert.Equal(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), result.TicketUid);
        Assert.Equal(new string('A', 80), result.Username);
        Assert.Equal("AFL999", result.FlightNumber);
        Assert.Equal(int.MaxValue, result.Price);
        Assert.Equal(TicketStatus.Canceled, result.Status);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        TicketPostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => TicketPostgresqlConverter.ToDomain(modelToConvert!)
        );
        Assert.Equal("model", exception.ParamName);
    }

    #endregion

    #region ToModel Tests

    [Fact]
    [Unit]
    public void ToModel_WithValidFullDomain_ShouldReturnModelWithAllProperties()
    {
        // Arrange
        var domain = TicketMother.CreateValidTicket();

        // Act
        var result = TicketPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.TicketUid, result.TicketUid);
        Assert.Equal(domain.Username, result.Username);
        Assert.Equal(domain.FlightNumber, result.FlightNumber);
        Assert.Equal(domain.Price, result.Price);
        Assert.Equal(domain.Status, result.Status);
    }

    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithMinValues_ShouldReturnModelWithMinValues()
    {
        // Arrange
        var domain = new Ticket
        {
            Id = 1,
            TicketUid = Guid.Empty,
            Username = "A",
            FlightNumber = "AFL001",
            Price = 0,
            Status = TicketStatus.Paid
        };

        // Act
        var result = TicketPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.TicketUid);
        Assert.Equal("A", result.Username);
        Assert.Equal("AFL001", result.FlightNumber);
        Assert.Equal(0, result.Price);
        Assert.Equal(TicketStatus.Paid, result.Status);
    }

    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        TicketDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => TicketPostgresqlConverter.ToModel(domain!)
        );
        Assert.Equal("domain", exception.ParamName);
    }

    #endregion

    #region ToDomainList Tests

    [Fact]
    [Unit]
    public void ToDomainList_WithValidCollection_ShouldReturnListOfDomains()
    {
        // Arrange
        var tickets = TicketMother.CreateTicketList(5);
        var models = tickets.Select(t => new TicketPostgresqlModel
        {
            Id = t.Id,
            TicketUid = t.TicketUid,
            Username = t.Username,
            FlightNumber = t.FlightNumber,
            Price = t.Price,
            Status = t.Status
        }).ToList();

        // Act
        var result = TicketPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].Username, result[i].Username);
            Assert.Equal(models[i].Price, result[i].Price);
        }
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var models = new List<TicketPostgresqlModel>();

        // Act
        var result = TicketPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<TicketPostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => TicketPostgresqlConverter.ToDomainList(models!)
        );
        Assert.Equal("models", exception.ParamName);
    }

    #endregion

    #region ToModelList Tests

    [Fact]
    [Unit]
    public void ToModelList_WithValidCollection_ShouldReturnListOfModels()
    {
        // Arrange
        var domains = TicketMother.CreateTicketList(3);

        // Act
        var result = TicketPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].Username, result[i].Username);
            Assert.Equal(domains[i].Price, result[i].Price);
        }
    }

    [Fact]
    [Unit]
    public void ToModelList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var domains = new List<TicketDomain>();

        // Act
        var result = TicketPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToModelList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<TicketDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => TicketPostgresqlConverter.ToModelList(domains!)
        );
        Assert.Equal("domains", exception.ParamName);
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    [Unit]
    public void RoundTrip_DomainToModelToDomain_ShouldPreserveData()
    {
        // Arrange
        var originalDomain = TicketMother.CreateValidTicket();

        // Act
        var model = TicketPostgresqlConverter.ToModel(originalDomain);
        var resultDomain = TicketPostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.TicketUid, resultDomain.TicketUid);
        Assert.Equal(originalDomain.Username, resultDomain.Username);
        Assert.Equal(originalDomain.FlightNumber, resultDomain.FlightNumber);
        Assert.Equal(originalDomain.Price, resultDomain.Price);
        Assert.Equal(originalDomain.Status, resultDomain.Status);
    }

    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new TicketPostgresqlModel
        {
            Id = 1,
            TicketUid = Guid.NewGuid(),
            Username = "john_doe",
            FlightNumber = "AFL031",
            Price = 15000,
            Status = TicketStatus.Paid
        };

        // Act
        var domain = TicketPostgresqlConverter.ToDomain(originalModel);
        var resultModel = TicketPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.TicketUid, resultModel.TicketUid);
        Assert.Equal(originalModel.Username, resultModel.Username);
        Assert.Equal(originalModel.FlightNumber, resultModel.FlightNumber);
        Assert.Equal(originalModel.Price, resultModel.Price);
        Assert.Equal(originalModel.Status, resultModel.Status);
    }

    #endregion
}
