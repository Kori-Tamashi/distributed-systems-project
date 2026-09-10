using System.Reflection;
using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;

using PersonDomain = core.domain.Person;
using PersonPostgresqlModel = dataaccess.models.postgres.PersonPostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for PersonPostgresqlConverter
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDomain(PersonPostgresqlModel):
/// - Valid model with all fields populated (normal case)
/// - Valid model with null optional fields (Age, Address, Work = null)
/// - Valid model with empty string values
/// - Null model (exception case - should throw ArgumentNullException)
/// 
/// For ToModel(PersonDomain):
/// - Valid domain with all fields populated (normal case)
/// - Valid domain with null optional fields (Age, Address, Work = null)
/// - Valid domain with empty string values
/// - Null domain (exception case - should throw ArgumentNullException)
/// 
/// For ToDomainList(IEnumerable&lt;PersonPostgresqlModel&gt;):
/// - Non-empty collection with valid models (normal case)
/// - Empty collection (edge case - should return empty list)
/// - Collection with null elements (exception case)
/// - Null collection (exception case - should throw ArgumentNullException)
/// 
/// For ToModelList(IEnumerable&lt;PersonDomain&gt;):
/// - Non-empty collection with valid domains (normal case)
/// - Empty collection (edge case - should return empty list)
/// - Collection with null elements (exception case)
/// - Null collection (exception case - should throw ArgumentNullException)
/// </summary>
public class PersonPostgresqlConverterUnitTests
{
    #region ToDomain Tests

    /// <summary>
    /// EP1: Valid model with all fields populated
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidFullModel_ShouldReturnDomainWithAllProperties()
    {
        // Arrange
        var model = PersonMother.CreateValidPerson();
        var modelToConvert = new PersonPostgresqlModel
        {
            Id = model.Id,
            Name = model.Name,
            Age = model.Age,
            Address = model.Address,
            Work = model.Work
        };

        // Act
        var result = PersonPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.Name, result.Name);
        Assert.Equal(modelToConvert.Age, result.Age);
        Assert.Equal(modelToConvert.Address, result.Address);
        Assert.Equal(modelToConvert.Work, result.Work);
    }

    /// <summary>
    /// EP2: Valid model with null optional fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithNulls_ShouldReturnDomainWithNulls()
    {
        // Arrange
        var modelToConvert = new PersonPostgresqlModel
        {
            Id = 1,
            Name = "John Doe",
            Age = null,
            Address = null,
            Work = null
        };

        // Act
        var result = PersonPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Null(result.Age);
        Assert.Null(result.Address);
        Assert.Null(result.Work);
    }

    /// <summary>
    /// EP3: Valid model with empty strings
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithModelWithEmptyStrings_ShouldReturnDomainWithEmptyStrings()
    {
        // Arrange
        var modelToConvert = new PersonPostgresqlModel
        {
            Id = 1,
            Name = "",
            Age = 0,
            Address = "",
            Work = ""
        };

        // Act
        var result = PersonPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Name);
        Assert.Equal(0, result.Age);
        Assert.Equal("", result.Address);
        Assert.Equal("", result.Work);
    }

    /// <summary>
    /// EP4: Null model (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        PersonPostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PersonPostgresqlConverter.ToDomain(modelToConvert!)
        );
        Assert.Equal("model", exception.ParamName);
    }

    #endregion

    #region ToModel Tests

    /// <summary>
    /// EP1: Valid domain with all fields populated
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithValidFullDomain_ShouldReturnModelWithAllProperties()
    {
        // Arrange
        var domain = PersonMother.CreateValidPerson();

        // Act
        var result = PersonPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.Name, result.Name);
        Assert.Equal(domain.Age, result.Age);
        Assert.Equal(domain.Address, result.Address);
        Assert.Equal(domain.Work, result.Work);
    }

    /// <summary>
    /// EP2: Valid domain with null optional fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithNulls_ShouldReturnModelWithNulls()
    {
        // Arrange
        var domain = PersonMother.CreatePersonWithNulls();

        // Act
        var result = PersonPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.Name, result.Name);
        Assert.Null(result.Age);
        Assert.Null(result.Address);
        Assert.Null(result.Work);
    }

    /// <summary>
    /// EP3: Null domain (exception case)
    /// </summary>
    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        PersonDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PersonPostgresqlConverter.ToModel(domain!)
        );
        Assert.Equal("domain", exception.ParamName);
    }

    #endregion

    #region ToDomainList Tests

    /// <summary>
    /// EP1: Non-empty collection with valid models
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomainList_WithValidCollection_ShouldReturnListOfDomains()
    {
        // Arrange
        var models = PersonMother.CreatePersonList(5)
            .Select(p => new PersonPostgresqlModel
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                Address = p.Address,
                Work = p.Work
            })
            .ToList();

        // Act
        var result = PersonPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].Name, result[i].Name);
            Assert.Equal(models[i].Age, result[i].Age);
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
        var models = new List<PersonPostgresqlModel>();

        // Act
        var result = PersonPostgresqlConverter.ToDomainList(models);

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
        IEnumerable<PersonPostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PersonPostgresqlConverter.ToDomainList(models!)
        );
        Assert.Equal("models", exception.ParamName);
    }

    #endregion

    #region ToModelList Tests

    /// <summary>
    /// EP1: Non-empty collection with valid domains
    /// </summary>
    [Fact]
    [Unit]
    public void ToModelList_WithValidCollection_ShouldReturnListOfModels()
    {
        // Arrange
        var domains = PersonMother.CreatePersonList(3);

        // Act
        var result = PersonPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].Name, result[i].Name);
            Assert.Equal(domains[i].Age, result[i].Age);
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
        var domains = new List<PersonDomain>();

        // Act
        var result = PersonPostgresqlConverter.ToModelList(domains);

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
        IEnumerable<PersonDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PersonPostgresqlConverter.ToModelList(domains!)
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
        var originalDomain = PersonMother.CreateValidPerson();

        // Act
        var model = PersonPostgresqlConverter.ToModel(originalDomain);
        var resultDomain = PersonPostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.Name, resultDomain.Name);
        Assert.Equal(originalDomain.Age, resultDomain.Age);
        Assert.Equal(originalDomain.Address, resultDomain.Address);
        Assert.Equal(originalDomain.Work, resultDomain.Work);
    }

    /// <summary>
    /// Round-trip conversion: Model -> Domain -> Model should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new PersonPostgresqlModel
        {
            Id = 1,
            Name = "Test Person",
            Age = 25,
            Address = "Test Address",
            Work = "Test Work"
        };

        // Act
        var domain = PersonPostgresqlConverter.ToDomain(originalModel);
        var resultModel = PersonPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.Name, resultModel.Name);
        Assert.Equal(originalModel.Age, resultModel.Age);
        Assert.Equal(originalModel.Address, resultModel.Address);
        Assert.Equal(originalModel.Work, resultModel.Work);
    }

    #endregion
}
