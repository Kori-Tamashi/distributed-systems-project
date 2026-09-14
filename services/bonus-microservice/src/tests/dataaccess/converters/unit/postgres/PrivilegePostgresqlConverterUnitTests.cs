using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using core.enums;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;
using Xunit;

using PrivilegeDomain = core.domain.Privilege;
using PrivilegePostgresqlModel = dataaccess.models.postgres.PrivilegePostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for PrivilegePostgresqlConverter
/// </summary>
public class PrivilegePostgresqlConverterUnitTests
{
    #region ToDomain Tests

    [Fact]
    [Unit]
    public void ToDomain_WithValidFullModel_ShouldReturnDomainWithAllProperties()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var modelToConvert = new PrivilegePostgresqlModel
        {
            Id = privilege.Id,
            Username = privilege.Username,
            Status = (int)privilege.Status,
            Balance = privilege.Balance
        };

        // Act
        var result = PrivilegePostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.Username, result.Username);
        Assert.Equal(modelToConvert.Status, (int)result.Status);
        Assert.Equal(modelToConvert.Balance, result.Balance);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMinValues_ShouldReturnDomainWithMinValues()
    {
        // Arrange
        var modelToConvert = new PrivilegePostgresqlModel
        {
            Id = 1,
            Username = "a",
            Status = (int)PrivilegeStatus.BRONZE,
            Balance = 0
        };

        // Act
        var result = PrivilegePostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("a", result.Username);
        Assert.Equal(PrivilegeStatus.BRONZE, result.Status);
        Assert.Equal(0, result.Balance);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMaxValues_ShouldReturnDomainWithMaxValues()
    {
        // Arrange
        var modelToConvert = new PrivilegePostgresqlModel
        {
            Id = int.MaxValue,
            Username = new string('A', 80),
            Status = (int)PrivilegeStatus.GOLD,
            Balance = int.MaxValue
        };

        // Act
        var result = PrivilegePostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(int.MaxValue, result.Id);
        Assert.Equal(new string('A', 80), result.Username);
        Assert.Equal(PrivilegeStatus.GOLD, result.Status);
        Assert.Equal(int.MaxValue, result.Balance);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        PrivilegePostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegePostgresqlConverter.ToDomain(modelToConvert!)
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
        var domain = PrivilegeMother.CreateValidPrivilege();

        // Act
        var result = PrivilegePostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.Username, result.Username);
        Assert.Equal((int)domain.Status, result.Status);
        Assert.Equal(domain.Balance, result.Balance);
    }

    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithMinValues_ShouldReturnModelWithMinValues()
    {
        // Arrange
        var domain = new Privilege
        {
            Id = 1,
            Username = "a",
            Status = PrivilegeStatus.BRONZE,
            Balance = 0
        };

        // Act
        var result = PrivilegePostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("a", result.Username);
        Assert.Equal((int)PrivilegeStatus.BRONZE, result.Status);
        Assert.Equal(0, result.Balance);
    }

    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        PrivilegeDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegePostgresqlConverter.ToModel(domain!)
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
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        var models = privileges.Select(p => new PrivilegePostgresqlModel
        {
            Id = p.Id,
            Username = p.Username,
            Status = (int)p.Status,
            Balance = p.Balance
        }).ToList();

        // Act
        var result = PrivilegePostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].Username, result[i].Username);
            Assert.Equal(models[i].Balance, result[i].Balance);
        }
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var models = new List<PrivilegePostgresqlModel>();

        // Act
        var result = PrivilegePostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<PrivilegePostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegePostgresqlConverter.ToDomainList(models!)
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
        var domains = PrivilegeMother.CreatePrivilegeList(3);

        // Act
        var result = PrivilegePostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].Username, result[i].Username);
            Assert.Equal(domains[i].Balance, result[i].Balance);
        }
    }

    [Fact]
    [Unit]
    public void ToModelList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var domains = new List<PrivilegeDomain>();

        // Act
        var result = PrivilegePostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToModelList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<PrivilegeDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegePostgresqlConverter.ToModelList(domains!)
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
        var originalDomain = PrivilegeMother.CreateValidPrivilege();

        // Act
        var model = PrivilegePostgresqlConverter.ToModel(originalDomain);
        var resultDomain = PrivilegePostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.Username, resultDomain.Username);
        Assert.Equal(originalDomain.Status, resultDomain.Status);
        Assert.Equal(originalDomain.Balance, resultDomain.Balance);
    }

    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new PrivilegePostgresqlModel
        {
            Id = 1,
            Username = "john.doe",
            Status = (int)PrivilegeStatus.SILVER,
            Balance = 5000
        };

        // Act
        var domain = PrivilegePostgresqlConverter.ToDomain(originalModel);
        var resultModel = PrivilegePostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.Username, resultModel.Username);
        Assert.Equal(originalModel.Status, resultModel.Status);
        Assert.Equal(originalModel.Balance, resultModel.Balance);
    }

    #endregion
}
