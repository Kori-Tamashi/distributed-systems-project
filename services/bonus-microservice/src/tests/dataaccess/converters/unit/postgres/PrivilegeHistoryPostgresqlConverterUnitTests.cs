using core.domain;
using dataaccess.converters.postgres;
using dataaccess.models.postgres;
using core.enums;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.mothers;
using Xunit;

using PrivilegeHistoryDomain = core.domain.PrivilegeHistory;
using PrivilegeHistoryPostgresqlModel = dataaccess.models.postgres.PrivilegeHistoryPostgresqlModel;

namespace tests.dataaccess.converters.unit;

/// <summary>
/// Unit tests for PrivilegeHistoryPostgresqlConverter
/// </summary>
public class PrivilegeHistoryPostgresqlConverterUnitTests
{
    #region ToDomain Tests

    [Fact]
    [Unit]
    public void ToDomain_WithValidFullModel_ShouldReturnDomainWithAllProperties()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        var modelToConvert = new PrivilegeHistoryPostgresqlModel
        {
            Id = history.Id,
            PrivilegeId = history.PrivilegeId,
            TicketUid = history.TicketUid,
            DateTime = history.DateTime,
            BalanceDiff = history.BalanceDiff,
            OperationType = (int)history.OperationType
        };

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modelToConvert.Id, result.Id);
        Assert.Equal(modelToConvert.PrivilegeId, result.PrivilegeId);
        Assert.Equal(modelToConvert.TicketUid, result.TicketUid);
        Assert.Equal(modelToConvert.DateTime, result.DateTime);
        Assert.Equal(modelToConvert.BalanceDiff, result.BalanceDiff);
        Assert.Equal(modelToConvert.OperationType, (int)result.OperationType);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithValidModelWithMinValues_ShouldReturnDomainWithMinValues()
    {
        // Arrange
        var modelToConvert = new PrivilegeHistoryPostgresqlModel
        {
            Id = 1,
            PrivilegeId = 1,
            TicketUid = Guid.Empty,
            DateTime = DateTime.MinValue,
            BalanceDiff = 1,
            OperationType = (int)OperationType.FILL_IN_BALANCE
        };

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToDomain(modelToConvert);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.PrivilegeId);
        Assert.Equal(Guid.Empty, result.TicketUid);
        Assert.Equal(DateTime.MinValue, result.DateTime);
        Assert.Equal(1, result.BalanceDiff);
        Assert.Equal(OperationType.FILL_IN_BALANCE, result.OperationType);
    }

    [Fact]
    [Unit]
    public void ToDomain_WithNullModel_ShouldThrowArgumentNullException()
    {
        // Arrange
        PrivilegeHistoryPostgresqlModel? modelToConvert = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegeHistoryPostgresqlConverter.ToDomain(modelToConvert!)
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
        var domain = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(domain.Id, result.Id);
        Assert.Equal(domain.PrivilegeId, result.PrivilegeId);
        Assert.Equal(domain.TicketUid, result.TicketUid);
        Assert.Equal(domain.DateTime, result.DateTime);
        Assert.Equal(domain.BalanceDiff, result.BalanceDiff);
        Assert.Equal((int)domain.OperationType, result.OperationType);
    }

    [Fact]
    [Unit]
    public void ToModel_WithValidDomainWithMinValues_ShouldReturnModelWithMinValues()
    {
        // Arrange
        var domain = new PrivilegeHistory
        {
            Id = 1,
            PrivilegeId = 1,
            TicketUid = Guid.Empty,
            DateTime = DateTime.MinValue,
            BalanceDiff = 1,
            OperationType = OperationType.FILL_IN_BALANCE
        };

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.PrivilegeId);
        Assert.Equal(Guid.Empty, result.TicketUid);
        Assert.Equal(DateTime.MinValue, result.DateTime);
        Assert.Equal(1, result.BalanceDiff);
        Assert.Equal((int)OperationType.FILL_IN_BALANCE, result.OperationType);
    }

    [Fact]
    [Unit]
    public void ToModel_WithNullDomain_ShouldThrowArgumentNullException()
    {
        // Arrange
        PrivilegeHistoryDomain? domain = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegeHistoryPostgresqlConverter.ToModel(domain!)
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
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        var models = histories.Select(h => new PrivilegeHistoryPostgresqlModel
        {
            Id = h.Id,
            PrivilegeId = h.PrivilegeId,
            TicketUid = h.TicketUid,
            DateTime = h.DateTime,
            BalanceDiff = h.BalanceDiff,
            OperationType = (int)h.OperationType
        }).ToList();

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        for (int i = 0; i < models.Count; i++)
        {
            Assert.Equal(models[i].Id, result[i].Id);
            Assert.Equal(models[i].PrivilegeId, result[i].PrivilegeId);
            Assert.Equal(models[i].BalanceDiff, result[i].BalanceDiff);
        }
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var models = new List<PrivilegeHistoryPostgresqlModel>();

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToDomainList(models);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToDomainList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<PrivilegeHistoryPostgresqlModel>? models = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegeHistoryPostgresqlConverter.ToDomainList(models!)
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
        var domains = PrivilegeHistoryMother.CreateHistoryList(3);

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        for (int i = 0; i < domains.Count; i++)
        {
            Assert.Equal(domains[i].Id, result[i].Id);
            Assert.Equal(domains[i].PrivilegeId, result[i].PrivilegeId);
            Assert.Equal(domains[i].BalanceDiff, result[i].BalanceDiff);
        }
    }

    [Fact]
    [Unit]
    public void ToModelList_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var domains = new List<PrivilegeHistoryDomain>();

        // Act
        var result = PrivilegeHistoryPostgresqlConverter.ToModelList(domains);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    [Unit]
    public void ToModelList_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Arrange
        IEnumerable<PrivilegeHistoryDomain>? domains = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => PrivilegeHistoryPostgresqlConverter.ToModelList(domains!)
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
        var originalDomain = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var model = PrivilegeHistoryPostgresqlConverter.ToModel(originalDomain);
        var resultDomain = PrivilegeHistoryPostgresqlConverter.ToDomain(model);

        // Assert
        Assert.Equal(originalDomain.Id, resultDomain.Id);
        Assert.Equal(originalDomain.PrivilegeId, resultDomain.PrivilegeId);
        Assert.Equal(originalDomain.TicketUid, resultDomain.TicketUid);
        Assert.Equal(originalDomain.DateTime, resultDomain.DateTime);
        Assert.Equal(originalDomain.BalanceDiff, resultDomain.BalanceDiff);
        Assert.Equal(originalDomain.OperationType, resultDomain.OperationType);
    }

    [Fact]
    [Unit]
    public void RoundTrip_ModelToDomainToModel_ShouldPreserveData()
    {
        // Arrange
        var originalModel = new PrivilegeHistoryPostgresqlModel
        {
            Id = 1,
            PrivilegeId = 1,
            TicketUid = Guid.NewGuid(),
            DateTime = DateTime.Now,
            BalanceDiff = 500,
            OperationType = (int)OperationType.DEBIT_THE_ACCOUNT
        };

        // Act
        var domain = PrivilegeHistoryPostgresqlConverter.ToDomain(originalModel);
        var resultModel = PrivilegeHistoryPostgresqlConverter.ToModel(domain);

        // Assert
        Assert.Equal(originalModel.Id, resultModel.Id);
        Assert.Equal(originalModel.PrivilegeId, resultModel.PrivilegeId);
        Assert.Equal(originalModel.TicketUid, resultModel.TicketUid);
        Assert.Equal(originalModel.DateTime, resultModel.DateTime);
        Assert.Equal(originalModel.BalanceDiff, resultModel.BalanceDiff);
        Assert.Equal(originalModel.OperationType, resultModel.OperationType);
    }

    #endregion
}
