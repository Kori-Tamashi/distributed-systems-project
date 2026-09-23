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
/// Unit tests for TicketPostgresqlConverter
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

        // Act
        var result = TicketPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.TicketUid, result.TicketUid);
        Assert.Equal(modelToConvert.FlightId, result.FlightId);
        Assert.Equal(modelToConvert.PassengerName, result.PassengerName);
        Assert.Equal(modelToConvert.PassengerEmail, result.PassengerEmail);
        Assert.Equal(modelToConvert.PassengerPhone, result.PassengerPhone);
        Assert.Equal(modelToConvert.SeatNumber, result.SeatNumber);
        Assert.Equal(modelToConvert.Class, (int)result.Class);
        Assert.Equal(modelToConvert.Price, result.Price);
        Assert.Equal(modelToConvert.BookingDate, result.BookingDate);
        Assert.Equal(modelToConvert.Status, (int)result.Status);
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
            FlightId = 1,
            PassengerName = "A",
            PassengerEmail = "a@b.com",
            PassengerPhone = "123",
            SeatNumber = "1A",
            Class = (int)TicketClass.Economy,
            Price = 0,
            BookingDate = DateTime.MinValue,
            Status = (int)TicketStatus.Confirmed
        };

        // Act
        var result = TicketPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.TicketUid);
        Assert.Equal(1, result.FlightId);
        Assert.Equal("A", result.PassengerName);
        Assert.Equal("a@b.com", result.PassengerEmail);
        Assert.Equal("123", result.PassengerPhone);
        Assert.Equal("1A", result.SeatNumber);
        Assert.Equal(TicketClass.Economy, result.Class);
        Assert.Equal(0, result.Price);
        Assert.Equal(DateTime.MinValue, result.BookingDate);
        Assert.Equal(TicketStatus.Confirmed, result.Status);
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
            FlightId = int.MaxValue,
            PassengerName = new string('A', 255),
            PassengerEmail = new string('B', 255),
            PassengerPhone = new string('C', 50),
            SeatNumber = "99Z",
            Class = (int)TicketClass.First,
            Price = int.MaxValue,
            BookingDate = DateTime.MaxValue,
            Status = (int)TicketStatus.Refunded
        };

        // Act
        var result = TicketPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Id);
        Assert.Equal(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), result.TicketUid);
        Assert.Equal(int.MaxValue, result.FlightId);
        Assert.Equal(new string('A', 255), result.PassengerName);
        Assert.Equal(new string('B', 255), result.PassengerEmail);
        Assert.Equal(new string('C', 50), result.PassengerPhone);
        Assert.Equal("99Z", result.SeatNumber);
        Assert.Equal(TicketClass.First, result.Class);
        Assert.Equal(int.MaxValue, result.Price);
        Assert.Equal(DateTime.MaxValue, result.BookingDate);
        Assert.Equal(TicketStatus.Refunded, result.Status);
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
        Assert.Equal(domain.FlightId, result.FlightId);
        Assert.Equal(domain.PassengerName, result.PassengerName);
        Assert.Equal(domain.PassengerEmail, result.PassengerEmail);
        Assert.Equal(domain.PassengerPhone, result.PassengerPhone);
        Assert.Equal(domain.SeatNumber, result.SeatNumber);
        Assert.Equal((int)domain.Class, result.Class);
        Assert.Equal(domain.Price, result.Price);
        Assert.Equal(domain.BookingDate, result.BookingDate);
        Assert.Equal((int)domain.Status, result.Status);
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
            FlightId = 1,
            PassengerName = "A",
            PassengerEmail = "a@b.com",
            PassengerPhone = "123",
            SeatNumber = "1A",
            Class = TicketClass.Economy,
            Price = 0,
            BookingDate = DateTime.MinValue,
            Status = TicketStatus.Confirmed
        };

        // Act
        var result = TicketPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.TicketUid);
        Assert.Equal(1, result.FlightId);
        Assert.Equal("A", result.PassengerName);
        Assert.Equal("a@b.com", result.PassengerEmail);
        Assert.Equal("123", result.PassengerPhone);
        Assert.Equal("1A", result.SeatNumber);
        Assert.Equal((int)TicketClass.Economy, result.Class);
        Assert.Equal(0, result.Price);
        Assert.Equal(DateTime.MinValue, result.BookingDate);
        Assert.Equal((int)TicketStatus.Confirmed, result.Status);
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
            FlightId = t.FlightId,
            PassengerName = t.PassengerName,
            PassengerEmail = t.PassengerEmail,
            PassengerPhone = t.PassengerPhone,
            SeatNumber = t.SeatNumber,
            Class = (int)t.Class,
            Price = t.Price,
            BookingDate = t.BookingDate,
            Status = (int)t.Status
        }).ToList();

        // Act
        var result = TicketPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].PassengerName, result[i].PassengerName);
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
            Assert.Equal(domains[i].PassengerName, result[i].PassengerName);
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
        Assert.Equal(originalDomain.FlightId, resultDomain.FlightId);
        Assert.Equal(originalDomain.PassengerName, resultDomain.PassengerName);
        Assert.Equal(originalDomain.PassengerEmail, resultDomain.PassengerEmail);
        Assert.Equal(originalDomain.SeatNumber, resultDomain.SeatNumber);
        Assert.Equal(originalDomain.Class, resultDomain.Class);
        Assert.Equal(originalDomain.Price, resultDomain.Price);
        Assert.Equal(originalDomain.BookingDate, resultDomain.BookingDate);
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
            FlightId = 1,
            PassengerName = "John Doe",
            PassengerEmail = "john@example.com",
            PassengerPhone = "+79001234567",
            SeatNumber = "12A",
            Class = (int)TicketClass.Business,
            Price = 45000,
            BookingDate = DateTime.Now,
            Status = (int)TicketStatus.Confirmed
        };

        // Act
        var domain = TicketPostgresqlConverter.ToDomain(originalModel);
        var resultModel = TicketPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.TicketUid, resultModel.TicketUid);
        Assert.Equal(originalModel.FlightId, resultModel.FlightId);
        Assert.Equal(originalModel.PassengerName, resultModel.PassengerName);
        Assert.Equal(originalModel.PassengerEmail, resultModel.PassengerEmail);
        Assert.Equal(originalModel.SeatNumber, resultModel.SeatNumber);
        Assert.Equal(originalModel.Class, resultModel.Class);
        Assert.Equal(originalModel.Price, resultModel.Price);
        Assert.Equal(originalModel.BookingDate, resultModel.BookingDate);
        Assert.Equal(originalModel.Status, resultModel.Status);
    }

    #endregion
}
