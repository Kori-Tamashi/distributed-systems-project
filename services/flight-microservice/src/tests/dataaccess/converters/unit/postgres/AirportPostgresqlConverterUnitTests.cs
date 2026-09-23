using System.Reflection;
using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;

using AirportDomain = core.domain.Airport;
using AirportPostgresqlModel = dataaccess.models.postgres.AirportPostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for AirportPostgresqlConverter
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDomain(AirportPostgresqlModel):
/// - Valid model with all fields populated (normal case)
/// - Valid model with minimum values (edge case)
/// - Valid model with maximum values (edge case)
/// - Null model (exception case - should throw ArgumentNullException)
/// 
/// For ToModel(AirportDomain):
/// - Valid domain with all fields populated (normal case)
/// - Valid domain with minimum values (edge case)
/// - Valid domain with maximum values (edge case)
/// - Null domain (exception case - should throw ArgumentNullException)
/// 
/// For ToDomainList(IEnumerable&lt;AirportPostgresqlModel&gt;):
/// - Non-empty collection with valid models (normal case)
/// - Empty collection (edge case - should return empty list)
/// - Null collection (exception case - should throw ArgumentNullException)
/// 
/// For ToModelList(IEnumerable&lt;AirportDomain&gt;):
/// - Non-empty collection with valid domains (normal case)
/// - Empty collection (edge case - should return empty list)
/// - Null collection (exception case - should throw ArgumentNullException)
/// </summary>
public class AirportPostgresqlConverterUnitTests
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
        var airport = AirportMother.CreateValidAirport();
        var modelToConvert = new AirportPostgresqlModel
        {
            Id = airport.Id,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country
        };

        // Act
        var result = AirportPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.Name, result.Name);
        Assert.Equal(modelToConvert.City, result.City);
        Assert.Equal(modelToConvert.Country, result.Country);
    }

    /// <summary>
    /// EP2: Valid model with minimum values (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMinValues_ShouldReturnDomainWithMinValues()
    {
        // Arrange
        var modelToConvert = new AirportPostgresqlModel
        {
            Id = 1,
            Name = "A",
            City = "C",
            Country = "R"
        };

        // Act
        var result = AirportPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("A", result.Name);
        Assert.Equal("C", result.City);
        Assert.Equal("R", result.Country);
    }

    /// <summary>
    /// EP3: Valid model with maximum values (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMaxValues_ShouldReturnDomainWithMaxValues()
    {
        // Arrange
        var modelToConvert = new AirportPostgresqlModel
        {
            Id = int.MaxValue,
            Name = new string('A', 255),
            City = new string('B', 255),
            Country = new string('C', 255)
        };

        // Act
        var result = AirportPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Id);
        Assert.Equal(new string('A', 255), result.Name);
        Assert.Equal(new string('B', 255), result.City);
        Assert.Equal(new string('C', 255), result.Country);
    }

    /// <summary>
    /// EP4: Null model (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        AirportPostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => AirportPostgresqlConverter.ToDomain(modelToConvert!)
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
        var domain = AirportMother.CreateValidAirport();

        // Act
        var result = AirportPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.Name, result.Name);
        Assert.Equal(domain.City, result.City);
        Assert.Equal(domain.Country, result.Country);
    }

    /// <summary>
    /// EP2: Valid domain with minimum values (edge case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithMinValues_ShouldReturnModelWithMinValues()
    {
        // Arrange
        var domain = new Airport
        {
            Id = 1,
            Name = "A",
            City = "C",
            Country = "R"
        };

        // Act
        var result = AirportPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("A", result.Name);
        Assert.Equal("C", result.City);
        Assert.Equal("R", result.Country);
    }

    /// <summary>
    /// EP3: Null domain (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        AirportDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => AirportPostgresqlConverter.ToModel(domain!)
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
        var airports = AirportMother.CreateAirportList(5);
        var models = airports.Select(a => new AirportPostgresqlModel
        {
            Id = a.Id,
            Name = a.Name,
            City = a.City,
            Country = a.Country
        }).ToList();

        // Act
        var result = AirportPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].Name, result[i].Name);
            Assert.Equal(models[i].City, result[i].City);
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
        var models = new List<AirportPostgresqlModel>();

        // Act
        var result = AirportPostgresqlConverter.ToDomainList(models);

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
        IEnumerable<AirportPostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => AirportPostgresqlConverter.ToDomainList(models!)
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
        var domains = AirportMother.CreateAirportList(3);

        // Act
        var result = AirportPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].Name, result[i].Name);
            Assert.Equal(domains[i].City, result[i].City);
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
        var domains = new List<AirportDomain>();

        // Act
        var result = AirportPostgresqlConverter.ToModelList(domains);

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
        IEnumerable<AirportDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => AirportPostgresqlConverter.ToModelList(domains!)
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
        var originalDomain = AirportMother.CreateValidAirport();

        // Act
        var model = AirportPostgresqlConverter.ToModel(originalDomain);
        var resultDomain = AirportPostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.Name, resultDomain.Name);
        Assert.Equal(originalDomain.City, resultDomain.City);
        Assert.Equal(originalDomain.Country, resultDomain.Country);
    }

    /// <summary>
    /// Round-trip conversion: Model -> Domain -> Model should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new AirportPostgresqlModel
        {
            Id = 1,
            Name = "Sheremetyevo International Airport",
            City = "Moscow",
            Country = "Russia"
        };

        // Act
        var domain = AirportPostgresqlConverter.ToDomain(originalModel);
        var resultModel = AirportPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.Name, resultModel.Name);
        Assert.Equal(originalModel.City, resultModel.City);
        Assert.Equal(originalModel.Country, resultModel.Country);
    }

    #endregion
}
