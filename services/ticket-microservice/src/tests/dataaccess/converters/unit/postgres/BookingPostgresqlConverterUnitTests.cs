using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using core.enums;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;
using Xunit;

using BookingDomain = core.domain.Booking;
using BookingPostgresqlModel = dataaccess.models.postgres.BookingPostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for BookingPostgresqlConverter
/// </summary>
public class BookingPostgresqlConverterUnitTests
{
    #region ToDomain Tests

    [Fact]
    [Unit]
    public void ToDomain_WithValidFullModel_ShouldReturnDomainWithAllProperties()
    {
        // Arrange
        var booking = BookingMother.CreateValidBooking();
        var modelToConvert = new BookingPostgresqlModel
        {
            Id = booking.Id,
            BookingUid = booking.BookingUid,
            BookingReference = booking.BookingReference,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            Status = (int)booking.Status,
            PaymentMethod = (int)booking.PaymentMethod,
            PaymentTransactionId = booking.PaymentTransactionId
        };

        // Act
        var result = BookingPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.BookingUid, result.BookingUid);
        Assert.Equal(modelToConvert.BookingReference, result.BookingReference);
        Assert.Equal(modelToConvert.CustomerName, result.CustomerName);
        Assert.Equal(modelToConvert.CustomerEmail, result.CustomerEmail);
        Assert.Equal(modelToConvert.CustomerPhone, result.CustomerPhone);
        Assert.Equal(modelToConvert.TotalPrice, result.TotalPrice);
        Assert.Equal(modelToConvert.BookingDate, result.BookingDate);
        Assert.Equal(modelToConvert.Status, (int)result.Status);
        Assert.Equal(modelToConvert.PaymentMethod, (int)result.PaymentMethod);
        Assert.Equal(modelToConvert.PaymentTransactionId, result.PaymentTransactionId);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMinValues_ShouldReturnDomainWithMinValues()
    {
        // Arrange
        var modelToConvert = new BookingPostgresqlModel
        {
            Id = 1,
            BookingUid = Guid.Empty,
            BookingReference = "A1",
            CustomerName = "A",
            CustomerEmail = "a@b.com",
            CustomerPhone = "123",
            TotalPrice = 0,
            BookingDate = DateTime.MinValue,
            Status = (int)BookingStatus.Confirmed,
            PaymentMethod = (int)PaymentMethod.Cash,
            PaymentTransactionId = null
        };

        // Act
        var result = BookingPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.BookingUid);
        Assert.Equal("A1", result.BookingReference);
        Assert.Equal("A", result.CustomerName);
        Assert.Equal("a@b.com", result.CustomerEmail);
        Assert.Equal("123", result.CustomerPhone);
        Assert.Equal(0, result.TotalPrice);
        Assert.Equal(DateTime.MinValue, result.BookingDate);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.Equal(PaymentMethod.Cash, result.PaymentMethod);
        Assert.Null(result.PaymentTransactionId);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMaxValues_ShouldReturnDomainWithMaxValues()
    {
        // Arrange
        var modelToConvert = new BookingPostgresqlModel
        {
            Id = int.MaxValue,
            BookingUid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
            BookingReference = new string('A', 50),
            CustomerName = new string('B', 255),
            CustomerEmail = new string('C', 255),
            CustomerPhone = new string('D', 50),
            TotalPrice = int.MaxValue,
            BookingDate = DateTime.MaxValue,
            Status = (int)BookingStatus.Refunded,
            PaymentMethod = (int)PaymentMethod.Cash,
            PaymentTransactionId = new string('E', 255)
        };

        // Act
        var result = BookingPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Id);
        Assert.Equal(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), result.BookingUid);
        Assert.Equal(new string('A', 50), result.BookingReference);
        Assert.Equal(new string('B', 255), result.CustomerName);
        Assert.Equal(new string('C', 255), result.CustomerEmail);
        Assert.Equal(new string('D', 50), result.CustomerPhone);
        Assert.Equal(int.MaxValue, result.TotalPrice);
        Assert.Equal(DateTime.MaxValue, result.BookingDate);
        Assert.Equal(BookingStatus.Refunded, result.Status);
        Assert.Equal(PaymentMethod.Cash, result.PaymentMethod);
        Assert.Equal(new string('E', 255), result.PaymentTransactionId);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        BookingPostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => BookingPostgresqlConverter.ToDomain(modelToConvert!)
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
        var domain = BookingMother.CreateValidBooking();

        // Act
        var result = BookingPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.BookingUid, result.BookingUid);
        Assert.Equal(domain.BookingReference, result.BookingReference);
        Assert.Equal(domain.CustomerName, result.CustomerName);
        Assert.Equal(domain.CustomerEmail, result.CustomerEmail);
        Assert.Equal(domain.CustomerPhone, result.CustomerPhone);
        Assert.Equal(domain.TotalPrice, result.TotalPrice);
        Assert.Equal(domain.BookingDate, result.BookingDate);
        Assert.Equal((int)domain.Status, result.Status);
        Assert.Equal((int)domain.PaymentMethod, result.PaymentMethod);
        Assert.Equal(domain.PaymentTransactionId, result.PaymentTransactionId);
    }

    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithMinValues_ShouldReturnModelWithMinValues()
    {
        // Arrange
        var domain = new Booking
        {
            Id = 1,
            BookingUid = Guid.Empty,
            BookingReference = "A1",
            CustomerName = "A",
            CustomerEmail = "a@b.com",
            CustomerPhone = "123",
            TotalPrice = 0,
            BookingDate = DateTime.MinValue,
            Status = BookingStatus.Confirmed,
            PaymentMethod = PaymentMethod.Cash,
            PaymentTransactionId = null
        };

        // Act
        var result = BookingPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.BookingUid);
        Assert.Equal("A1", result.BookingReference);
        Assert.Equal("A", result.CustomerName);
        Assert.Equal("a@b.com", result.CustomerEmail);
        Assert.Equal("123", result.CustomerPhone);
        Assert.Equal(0, result.TotalPrice);
        Assert.Equal(DateTime.MinValue, result.BookingDate);
        Assert.Equal((int)BookingStatus.Confirmed, result.Status);
        Assert.Equal((int)PaymentMethod.Cash, result.PaymentMethod);
        Assert.Null(result.PaymentTransactionId);
    }

    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        BookingDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => BookingPostgresqlConverter.ToModel(domain!)
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
        var bookings = BookingMother.CreateBookingList(5);
        var models = bookings.Select(b => new BookingPostgresqlModel
        {
            Id = b.Id,
            BookingUid = b.BookingUid,
            BookingReference = b.BookingReference,
            CustomerName = b.CustomerName,
            CustomerEmail = b.CustomerEmail,
            CustomerPhone = b.CustomerPhone,
            TotalPrice = b.TotalPrice,
            BookingDate = b.BookingDate,
            Status = (int)b.Status,
            PaymentMethod = (int)b.PaymentMethod,
            PaymentTransactionId = b.PaymentTransactionId
        }).ToList();

        // Act
        var result = BookingPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].CustomerName, result[i].CustomerName);
            Assert.Equal(models[i].TotalPrice, result[i].TotalPrice);
        }
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var models = new List<BookingPostgresqlModel>();

        // Act
        var result = BookingPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<BookingPostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => BookingPostgresqlConverter.ToDomainList(models!)
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
        var domains = BookingMother.CreateBookingList(3);

        // Act
        var result = BookingPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].CustomerName, result[i].CustomerName);
            Assert.Equal(domains[i].TotalPrice, result[i].TotalPrice);
        }
    }

    [Fact]
    [Unit]
    public void ToModelList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var domains = new List<BookingDomain>();

        // Act
        var result = BookingPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToModelList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<BookingDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => BookingPostgresqlConverter.ToModelList(domains!)
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
        var originalDomain = BookingMother.CreateValidBooking();

        // Act
        var model = BookingPostgresqlConverter.ToModel(originalDomain);
        var resultDomain = BookingPostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.BookingUid, resultDomain.BookingUid);
        Assert.Equal(originalDomain.BookingReference, resultDomain.BookingReference);
        Assert.Equal(originalDomain.CustomerName, resultDomain.CustomerName);
        Assert.Equal(originalDomain.CustomerEmail, resultDomain.CustomerEmail);
        Assert.Equal(originalDomain.TotalPrice, resultDomain.TotalPrice);
        Assert.Equal(originalDomain.BookingDate, resultDomain.BookingDate);
        Assert.Equal(originalDomain.Status, resultDomain.Status);
        Assert.Equal(originalDomain.PaymentMethod, resultDomain.PaymentMethod);
        Assert.Equal(originalDomain.PaymentTransactionId, resultDomain.PaymentTransactionId);
    }

    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new BookingPostgresqlModel
        {
            Id = 1,
            BookingUid = Guid.NewGuid(),
            BookingReference = "ABC123",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            CustomerPhone = "+79001234567",
            TotalPrice = 45000,
            BookingDate = DateTime.Now,
            Status = (int)BookingStatus.Confirmed,
            PaymentMethod = (int)PaymentMethod.CreditCard,
            PaymentTransactionId = "TXN123456"
        };

        // Act
        var domain = BookingPostgresqlConverter.ToDomain(originalModel);
        var resultModel = BookingPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.BookingUid, resultModel.BookingUid);
        Assert.Equal(originalModel.BookingReference, resultModel.BookingReference);
        Assert.Equal(originalModel.CustomerName, resultModel.CustomerName);
        Assert.Equal(originalModel.CustomerEmail, resultModel.CustomerEmail);
        Assert.Equal(originalModel.TotalPrice, resultModel.TotalPrice);
        Assert.Equal(originalModel.BookingDate, resultModel.BookingDate);
        Assert.Equal(originalModel.Status, resultModel.Status);
        Assert.Equal(originalModel.PaymentMethod, resultModel.PaymentMethod);
        Assert.Equal(originalModel.PaymentTransactionId, resultModel.PaymentTransactionId);
    }

    #endregion
}
