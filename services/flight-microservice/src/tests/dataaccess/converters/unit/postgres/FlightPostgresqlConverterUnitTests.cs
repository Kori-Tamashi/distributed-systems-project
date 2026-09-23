using System.Reflection;
using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;

using FlightDomain = core.domain.Flight;
using FlightPostgresqlModel = dataaccess.models.postgres.FlightPostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for FlightPostgresqlConverter
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDomain(FlightPostgresqlModel):
/// - Valid model with all fields populated (normal case)
/// - Valid model with minimum values (edge case)
/// - Valid model with maximum values (edge case)
/// - Null model (exception case - should throw ArgumentNullException)
/// 
/// For ToModel(FlightDomain):
/// - Valid domain with all fields populated (normal case)
/// - Valid domain with minimum values (edge case)
/// - Valid domain with maximum values (edge case)
/// - Null domain (exception case - should throw ArgumentNullException)
/// 
/// For ToDomainList(IEnumerable&lt;FlightPostgresqlModel&gt;):
/// - Non-empty collection with valid models (normal case)
/// - Empty collection (edge case - should return empty list)
/// - Null collection (exception case - should throw ArgumentNullException)
/// 
/// For ToModelList(IEnumerable&lt;FlightDomain&gt;):
/// - Non-empty collection with valid domains (normal case)
/// - Empty collection (edge case - should return empty list)
/// - Null collection (exception case - should throw ArgumentNullException)
/// </summary>
public class FlightPostgresqlConverterUnitTests
{
    #region ToDomain Tests

    /// <summary>
    /// EP1: Valid model with all fields populated (normal case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidFullModel_ShouldReturnDomainWithAllProperties()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        var modelToConvert = new FlightPostgresqlModel
        {
            Id = flight.Id,
            FlightUid = flight.FlightUid,
            FlightNumber = flight.FlightNumber,
            DateTime = flight.DateTime,
            FromAirportId = flight.FromAirportId,
            ToAirportId = flight.ToAirportId,
            Price = flight.Price
        };

        // Act
        var result = FlightPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.FlightUid, result.FlightUid);
        Assert.Equal(modelToConvert.FlightNumber, result.FlightNumber);
        Assert.Equal(modelToConvert.DateTime, result.DateTime);
        Assert.Equal(modelToConvert.FromAirportId, result.FromAirportId);
        Assert.Equal(modelToConvert.ToAirportId, result.ToAirportId);
        Assert.Equal(modelToConvert.Price, result.Price);
    }

    /// <summary>
    /// EP2: Valid model with minimum values (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMinValues_ShouldReturnDomainWithMinValues()
    {
        // Arrange
        var modelToConvert = new FlightPostgresqlModel
        {
            Id = 1,
            FlightUid = Guid.Empty,
            FlightNumber = "A1",
            DateTime = DateTime.MinValue,
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 0
        };

        // Act
        var result = FlightPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.FlightUid);
        Assert.Equal("A1", result.FlightNumber);
        Assert.Equal(DateTime.MinValue, result.DateTime);
        Assert.Equal(1, result.FromAirportId);
        Assert.Equal(2, result.ToAirportId);
        Assert.Equal(0, result.Price);
    }

    /// <summary>
    /// EP3: Valid model with maximum values (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMaxValues_ShouldReturnDomainWithMaxValues()
    {
        // Arrange
        var modelToConvert = new FlightPostgresqlModel
        {
            Id = int.MaxValue,
            FlightUid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"),
            FlightNumber = "ABCDEFGHIJKLMNOPQR", // 20 chars
            DateTime = DateTime.MaxValue,
            FromAirportId = int.MaxValue,
            ToAirportId = int.MaxValue,
            Price = int.MaxValue
        };

        // Act
        var result = FlightPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Id);
        Assert.Equal(new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"), result.FlightUid);
        Assert.Equal("ABCDEFGHIJKLMNOPQR", result.FlightNumber);
        Assert.Equal(DateTime.MaxValue, result.DateTime);
        Assert.Equal(int.MaxValue, result.FromAirportId);
        Assert.Equal(int.MaxValue, result.ToAirportId);
        Assert.Equal(int.MaxValue, result.Price);
    }

    /// <summary>
    /// EP4: Null model (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        FlightPostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => FlightPostgresqlConverter.ToDomain(modelToConvert!)
        );
        Assert.Equal("model", exception.ParamName);
    }

    #endregion

    #region ToModel Tests

    /// <summary>
    /// EP1: Valid domain with all fields populated (normal case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithValidFullDomain_ShouldReturnModelWithAllProperties()
    {
        // Arrange
        var domain = FlightMother.CreateValidFlight();

        // Act
        var result = FlightPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.FlightUid, result.FlightUid);
        Assert.Equal(domain.FlightNumber, result.FlightNumber);
        Assert.Equal(domain.DateTime, result.DateTime);
        Assert.Equal(domain.FromAirportId, result.FromAirportId);
        Assert.Equal(domain.ToAirportId, result.ToAirportId);
        Assert.Equal(domain.Price, result.Price);
    }

    /// <summary>
    /// EP2: Valid domain with minimum values (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithMinValues_ShouldReturnModelWithMinValues()
    {
        // Arrange
        var domain = new Flight
        {
            Id = 1,
            FlightUid = Guid.Empty,
            FlightNumber = "A1",
            DateTime = DateTime.MinValue,
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 0
        };

        // Act
        var result = FlightPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(Guid.Empty, result.FlightUid);
        Assert.Equal("A1", result.FlightNumber);
        Assert.Equal(DateTime.MinValue, result.DateTime);
        Assert.Equal(1, result.FromAirportId);
        Assert.Equal(2, result.ToAirportId);
        Assert.Equal(0, result.Price);
    }

    /// <summary>
    /// EP3: Null domain (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        FlightDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => FlightPostgresqlConverter.ToModel(domain!)
        );
        Assert.Equal("domain", exception.ParamName);
    }

    #endregion

    #region ToDomainList Tests

    /// <summary>
    /// EP1: Non-empty collection with valid models (normal case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomainList_WithValidCollection_ShouldReturnListOfDomains()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        var models = flights.Select(f => new FlightPostgresqlModel
        {
            Id = f.Id,
            FlightUid = f.FlightUid,
            FlightNumber = f.FlightNumber,
            DateTime = f.DateTime,
            FromAirportId = f.FromAirportId,
            ToAirportId = f.ToAirportId,
            Price = f.Price
        }).ToList();

        // Act
        var result = FlightPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].FlightNumber, result[i].FlightNumber);
            Assert.Equal(models[i].Price, result[i].Price);
        }
    }

    /// <summary>
    /// EP2: Empty collection (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomainList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var models = new List<FlightPostgresqlModel>();

        // Act
        var result = FlightPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP3: Null collection (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomainList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<FlightPostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => FlightPostgresqlConverter.ToDomainList(models!)
        );
        Assert.Equal("models", exception.ParamName);
    }

    #endregion

    #region ToModelList Tests

    /// <summary>
    /// EP1: Non-empty collection with valid domains (normal case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModelList_WithValidCollection_ShouldReturnListOfModels()
    {
        // Arrange
        var domains = FlightMother.CreateFlightList(3);

        // Act
        var result = FlightPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].FlightNumber, result[i].FlightNumber);
            Assert.Equal(domains[i].Price, result[i].Price);
        }
    }

    /// <summary>
    /// EP2: Empty collection (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModelList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var domains = new List<FlightDomain>();

        // Act
        var result = FlightPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP3: Null collection (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModelList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<FlightDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => FlightPostgresqlConverter.ToModelList(domains!)
        );
        Assert.Equal("domains", exception.ParamName);
    }

    #endregion

    #region Round-trip Tests

    /// <summary>
    /// Round-trip conversion: Domain -> Model -> Domain should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void RoundTrip_DomainToModelToDomain_ShouldPreserveData()
    {
        // Arrange
        var originalDomain = FlightMother.CreateValidFlight();

        // Act
        var model = FlightPostgresqlConverter.ToModel(originalDomain);
        var resultDomain = FlightPostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.FlightUid, resultDomain.FlightUid);
        Assert.Equal(originalDomain.FlightNumber, resultDomain.FlightNumber);
        Assert.Equal(originalDomain.DateTime, resultDomain.DateTime);
        Assert.Equal(originalDomain.FromAirportId, resultDomain.FromAirportId);
        Assert.Equal(originalDomain.ToAirportId, resultDomain.ToAirportId);
        Assert.Equal(originalDomain.Price, resultDomain.Price);
    }

    /// <summary>
    /// Round-trip conversion: Model -> Domain -> Model should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new FlightPostgresqlModel
        {
            Id = 1,
            FlightUid = Guid.NewGuid(),
            FlightNumber = "SU1234",
            DateTime = DateTime.Now.AddHours(2),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 15000
        };

        // Act
        var domain = FlightPostgresqlConverter.ToDomain(originalModel);
        var resultModel = FlightPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.FlightUid, resultModel.FlightUid);
        Assert.Equal(originalModel.FlightNumber, resultModel.FlightNumber);
        Assert.Equal(originalModel.DateTime, resultModel.DateTime);
        Assert.Equal(originalModel.FromAirportId, resultModel.FromAirportId);
        Assert.Equal(originalModel.ToAirportId, resultModel.ToAirportId);
        Assert.Equal(originalModel.Price, resultModel.Price);
    }

    #endregion
}
