using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using presentation.controllers.http;
using presentation.converters.http;
using presentation.dto.http.PrivilegeHistory;
using presentation.exceptions.http;
using presentation.exceptions.http.PrivilegeHistory;
using tests.config.attributes;
using tests.fixtures.mothers;

using ServicePrivilegeHistoryNotFoundException = core.exceptions.businesslogic.services.PrivilegeHistoryNotFoundException;
using ServicePrivilegeHistoryValidationException = core.exceptions.businesslogic.services.PrivilegeHistoryValidationException;
using HttpPrivilegeHistoryNotFoundException = presentation.exceptions.http.PrivilegeHistory.PrivilegeHistoryNotFoundException;
using HttpPrivilegeHistoryValidationException = presentation.exceptions.http.PrivilegeHistory.PrivilegeHistoryValidationException;
using PrivilegeHistoryInternalServerException = presentation.exceptions.http.PrivilegeHistory.PrivilegeHistoryInternalServerException;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for PrivilegeHistoryHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetPrivilegeHistoryById(int privilegeHistoryId):
/// - EP1: Valid ID, PrivilegeHistory exists (200 OK)
/// - EP2: Valid ID, PrivilegeHistory not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllPrivilegeHistories():
/// - EP1: Returns all histories (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreatePrivilegeHistory(CreatePrivilegeHistoryDto):
/// - EP1: Valid history, creation successful (201 Created)
/// - EP2: Invalid history (400 Bad Request)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For UpdatePrivilegeHistory(int privilegeHistoryId, UpdatePrivilegeHistoryDto):
/// - EP1: Valid history, PrivilegeHistory exists (200 OK)
/// - EP2: PrivilegeHistory not found (404 Not Found)
/// - EP3: Invalid history (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeletePrivilegeHistory(int privilegeHistoryId):
/// - EP1: Valid ID, PrivilegeHistory exists (200 OK)
/// - EP2: Valid ID, PrivilegeHistory not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 17 unit tests (all should pass)
/// </summary>
public class PrivilegeHistoryHttpControllerUnitTests
{
    private readonly Mock<IPrivilegeHistoryService> _mockService;
    private readonly Mock<ILogger<PrivilegeHistoryHttpController>> _mockLogger;
    private readonly PrivilegeHistoryHttpController _controller;

    public PrivilegeHistoryHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<IPrivilegeHistoryService>();
        _mockLogger = new Mock<ILogger<PrivilegeHistoryHttpController>>();
        
        _controller = new PrivilegeHistoryHttpController(_mockService.Object, _mockLogger.Object);
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "PrivilegeHistory");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetPrivilegeHistoryById Tests

    /// <summary>
    /// EP1: Valid ID, PrivilegeHistory exists - should return 200 OK with PrivilegeHistory
    /// </summary>
    [Unit]
    public async Task GetPrivilegeHistoryById_ValidId_HistoryExists_ShouldReturnOk()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockService.Setup(s => s.GetByIdAsync(history.Id)).ReturnsAsync(history);

        // Act
        var result = await _controller.GetPrivilegeHistoryById(history.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeHistoryDTO>(okResult.Value);
        
        Assert.Equal(history.Id, dto.Id);
        _mockService.Verify(s => s.GetByIdAsync(history.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, PrivilegeHistory not found - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Unit]
    public async Task GetPrivilegeHistoryById_HistoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var historyId = 999;
        _mockService.Setup(s => s.GetByIdAsync(historyId))
            .ThrowsAsync(new ServicePrivilegeHistoryNotFoundException(historyId));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPrivilegeHistoryNotFoundException>(() => _controller.GetPrivilegeHistoryById(historyId));
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetPrivilegeHistoryById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var historyId = 1;
        _mockService.Setup(s => s.GetByIdAsync(historyId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.GetPrivilegeHistoryById(historyId));
    }

    #endregion

    #region GetAllPrivilegeHistories Tests

    /// <summary>
    /// EP1: Returns all histories - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetAllPrivilegeHistories_ReturnsAllHistories_ShouldReturnOk()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(histories);

        // Act
        var result = await _controller.GetAllPrivilegeHistories(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PrivilegeHistoryDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<PrivilegeHistoryDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Unit]
    public async Task GetAllPrivilegeHistories_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var histories = new List<core.domain.PrivilegeHistory>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(histories);

        // Act
        var result = await _controller.GetAllPrivilegeHistories(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PrivilegeHistoryDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<PrivilegeHistoryDTO>>(okResult.Value);
        
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetAllPrivilegeHistories_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.GetAllPrivilegeHistories(null, null));
    }

    #endregion

    #region CreatePrivilegeHistory Tests

    /// <summary>
    /// EP1: Valid history, creation successful - should return 201 Created
    /// </summary>
    [Unit]
    public async Task CreatePrivilegeHistory_ValidHistory_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO
        {
            PrivilegeId = 1,
            TicketUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow,
            BalanceDiff = 5000,
            OperationType = 0
        };
        var createdHistory = PrivilegeHistoryMother.CreateValidCreditHistory();
        createdHistory.Id = 1;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ReturnsAsync(createdHistory);

        // Act
        var result = await _controller.CreatePrivilegeHistory(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeHistoryDTO>(createdResult.Value);
        
        Assert.Equal(createdHistory.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid history - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Unit]
    public async Task CreatePrivilegeHistory_InvalidHistory_ShouldThrowValidationException()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new ServicePrivilegeHistoryValidationException("Invalid history"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPrivilegeHistoryValidationException>(() => _controller.CreatePrivilegeHistory(createDto));
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task CreatePrivilegeHistory_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.CreatePrivilegeHistory(createDto));
    }

    #endregion

    #region UpdatePrivilegeHistory Tests

    /// <summary>
    /// EP1: Valid history, PrivilegeHistory exists - should return 200 OK
    /// </summary>
    [Unit]
    public async Task UpdatePrivilegeHistory_ValidHistory_HistoryExists_ShouldReturnOk()
    {
        // Arrange
        var historyId = 1;
        var updateDto = new UpdatePrivilegeHistoryDTO
        {
            TicketUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow,
            BalanceDiff = 10000,
            OperationType = 1
        };
        var updatedHistory = PrivilegeHistoryMother.CreateValidCreditHistory();
        updatedHistory.Id = historyId;
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ReturnsAsync(updatedHistory);

        // Act
        var result = await _controller.UpdatePrivilegeHistory(historyId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeHistoryDTO>(okResult.Value);
        
        Assert.Equal(historyId, dto.Id);
    }

    /// <summary>
    /// EP2: PrivilegeHistory not found - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Unit]
    public async Task UpdatePrivilegeHistory_HistoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var historyId = 999;
        var updateDto = new UpdatePrivilegeHistoryDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new ServicePrivilegeHistoryNotFoundException(historyId));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPrivilegeHistoryNotFoundException>(() => _controller.UpdatePrivilegeHistory(historyId, updateDto));
    }

    /// <summary>
    /// EP3: Invalid history - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Unit]
    public async Task UpdatePrivilegeHistory_InvalidHistory_ShouldThrowValidationException()
    {
        // Arrange
        var historyId = 1;
        var updateDto = new UpdatePrivilegeHistoryDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new ServicePrivilegeHistoryValidationException("Invalid history"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPrivilegeHistoryValidationException>(() => _controller.UpdatePrivilegeHistory(historyId, updateDto));
    }

    /// <summary>
    /// EP4: ID mismatch - should throw PrivilegeHistoryInternalServerException (wraps validation)
    /// </summary>
    [Unit]
    public async Task UpdatePrivilegeHistory_IdMismatch_ShouldThrowInternalServerException()
    {
        // Arrange
        var routeHistoryId = 1;
        var updateDto = new UpdatePrivilegeHistoryDTO
        {
            TicketUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow,
            BalanceDiff = 10000,
            OperationType = 1
        };
        var existingHistory = PrivilegeHistoryMother.CreateValidCreditHistory();
        existingHistory.Id = 5; // Different ID from route
        _mockService.Setup(s => s.GetByIdAsync(routeHistoryId))
            .ReturnsAsync(existingHistory);

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.UpdatePrivilegeHistory(routeHistoryId, updateDto));
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task UpdatePrivilegeHistory_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var historyId = 1;
        var updateDto = new UpdatePrivilegeHistoryDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.UpdatePrivilegeHistory(historyId, updateDto));
    }

    #endregion

    #region DeletePrivilegeHistory Tests

    /// <summary>
    /// EP1: Valid ID, PrivilegeHistory exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task DeletePrivilegeHistory_ValidId_HistoryExists_ShouldReturnNoContent()
    {
        // Arrange
        var historyId = 1;
        _mockService.Setup(s => s.DeleteAsync(historyId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePrivilegeHistory(historyId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockService.Verify(s => s.DeleteAsync(historyId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, PrivilegeHistory not found - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Unit]
    public async Task DeletePrivilegeHistory_HistoryNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var historyId = 999;
        _mockService.Setup(s => s.DeleteAsync(historyId))
            .ThrowsAsync(new ServicePrivilegeHistoryNotFoundException(historyId));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPrivilegeHistoryNotFoundException>(() => _controller.DeletePrivilegeHistory(historyId));
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task DeletePrivilegeHistory_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var historyId = 1;
        _mockService.Setup(s => s.DeleteAsync(historyId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.DeletePrivilegeHistory(historyId));
    }

    #endregion
}
