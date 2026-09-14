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
using presentation.dto.http;
using presentation.dto.http.PrivilegeHistory;
using presentation.exceptions.http;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;
using CreatePrivilegeHistoryDTO = presentation.dto.http.PrivilegeHistory.CreatePrivilegeHistoryDTO;
using UpdatePrivilegeHistoryDTO = presentation.dto.http.PrivilegeHistory.UpdatePrivilegeHistoryDTO;

// Type aliases to avoid ambiguity
using ServicePrivilegeHistoryNotFoundException = core.exceptions.businesslogic.services.PrivilegeHistoryNotFoundException;
using ServicePrivilegeHistoryValidationException = core.exceptions.businesslogic.services.PrivilegeHistoryValidationException;
using ServicePrivilegeHistoryBusinessRuleViolationException = core.exceptions.businesslogic.services.PrivilegeHistoryBusinessRuleViolationException;
using HttpPrivilegeHistoryNotFoundException = presentation.exceptions.http.PrivilegeHistoryNotFoundException;
using HttpPrivilegeHistoryValidationException = presentation.exceptions.http.PrivilegeHistoryValidationException;
using HttpPrivilegeHistoryBusinessRuleViolationException = presentation.exceptions.http.PrivilegeHistoryBusinessRuleViolationException;
using PrivilegeHistoryInternalServerException = presentation.exceptions.http.PrivilegeHistoryInternalServerException;

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
/// - EP1: Valid ID, PrivilegeHistory exists (204 No Content)
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
    [Fact]
    [Unit]
    public async Task GetPrivilegeHistoryById_ValidId_PrivilegeHistoryExists_ShouldReturnOk()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockService.Setup(s => s.GetByIdAsync(history.Id)).ReturnsAsync(history);

        // Act
        var result = await _controller.GetPrivilegeHistoryById(history.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeHistoryDTO>(okResult.Value);
        
        Assert.Equal(history.Id, dto.Id);
        Assert.Equal(history.PrivilegeId, dto.PrivilegeId);
        _mockService.Verify(s => s.GetByIdAsync(history.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, PrivilegeHistory not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPrivilegeHistoryById_ValidId_PrivilegeHistoryNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var historyId = 999;
        _mockService.Setup(s => s.GetByIdAsync(historyId))
            .ThrowsAsync(new ServicePrivilegeHistoryNotFoundException(historyId));

        // Act
        var result = await _controller.GetPrivilegeHistoryById(historyId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpPrivilegeHistoryNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, dto.StatusCode);
        Assert.Equal("PRIVILEGE_HISTORY_NOT_FOUND", dto.ErrorCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPrivilegeHistoryById_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var historyId = 1;
        _mockService.Setup(s => s.GetByIdAsync(historyId))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.GetPrivilegeHistoryById(historyId));
    }

    #endregion

    #region GetAllPrivilegeHistories Tests

    /// <summary>
    /// EP1: Returns all histories - should return 200 OK with list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPrivilegeHistories_ShouldReturnOkWithList()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(histories);

        // Act
        var result = await _controller.GetAllPrivilegeHistories(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PrivilegeHistoryDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<PrivilegeHistoryDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPrivilegeHistories_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var histories = new List<core.domain.PrivilegeHistory>();
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(histories);

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
    [Fact]
    [Unit]
    public async Task GetAllPrivilegeHistories_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync())
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.GetAllPrivilegeHistories(null, null));
    }

    #endregion

    #region CreatePrivilegeHistory Tests

    /// <summary>
    /// EP1: Valid history, creation successful - should return 201 Created
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePrivilegeHistory_ValidHistory_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO(1, Guid.NewGuid(), DateTime.UtcNow, -100, 1);
        var createdHistory = PrivilegeHistoryMother.CreateValidHistory();
        createdHistory.Id = 10;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ReturnsAsync(createdHistory);

        // Act
        var result = await _controller.CreatePrivilegeHistory(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(201, createdResult.StatusCode);
        
        var dto = Assert.IsType<PrivilegeHistoryDTO>(createdResult.Value);
        Assert.Equal(createdHistory.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid history - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePrivilegeHistory_InvalidHistory_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO(0, Guid.NewGuid(), DateTime.UtcNow, -100, 1); // Invalid PrivilegeId
        _controller.ModelState.AddModelError("PrivilegeId", "PrivilegeId is required");

        // Act
        var result = await _controller.CreatePrivilegeHistory(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeHistoryValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePrivilegeHistory_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO(1, Guid.NewGuid(), DateTime.UtcNow, -100, 1);
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.CreatePrivilegeHistory(createDto));
    }

    #endregion

    #region UpdatePrivilegeHistory Tests

    /// <summary>
    /// EP1: Valid history, PrivilegeHistory exists - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilegeHistory_ValidHistory_PrivilegeHistoryExists_ShouldReturnOk()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        var updateDto = new UpdatePrivilegeHistoryDTO(history.Id, 2, history.TicketUid, DateTime.UtcNow, -200, 1);
        _mockService.Setup(s => s.GetByIdAsync(history.Id)).ReturnsAsync(history);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.UpdatePrivilegeHistory(history.Id, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeHistoryDTO>(okResult.Value);
        
        Assert.Equal(history.Id, dto.Id);
    }

    /// <summary>
    /// EP2: PrivilegeHistory not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilegeHistory_PrivilegeHistoryNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var historyId = 999;
        var updateDto = new UpdatePrivilegeHistoryDTO(historyId, 2, Guid.NewGuid(), DateTime.UtcNow, -200, 1);
        _mockService.Setup(s => s.GetByIdAsync(historyId))
            .ThrowsAsync(new ServicePrivilegeHistoryNotFoundException(historyId));

        // Act
        var result = await _controller.UpdatePrivilegeHistory(historyId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeHistoryNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Invalid history - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilegeHistory_InvalidHistory_ShouldReturnBadRequest()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        var updateDto = new UpdatePrivilegeHistoryDTO(history.Id, 0, history.TicketUid, DateTime.UtcNow, 0, 1); // Invalid BalanceDiff
        _controller.ModelState.AddModelError("BalanceDiff", "BalanceDiff cannot be zero");

        // Act
        var result = await _controller.UpdatePrivilegeHistory(history.Id, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeHistoryValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, exception.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilegeHistory_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        var updateDto = new UpdatePrivilegeHistoryDTO(999, 2, history.TicketUid, DateTime.UtcNow, -200, 1); // Different ID

        // Act
        var result = await _controller.UpdatePrivilegeHistory(history.Id, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeHistoryDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeHistoryValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, exception.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilegeHistory_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        var updateDto = new UpdatePrivilegeHistoryDTO(history.Id, 2, history.TicketUid, DateTime.UtcNow, -200, 1);
        _mockService.Setup(s => s.GetByIdAsync(history.Id)).ReturnsAsync(history);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.PrivilegeHistory>()))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.UpdatePrivilegeHistory(history.Id, updateDto));
    }

    #endregion

    #region DeletePrivilegeHistory Tests

    /// <summary>
    /// EP1: Valid ID, PrivilegeHistory exists - should return 204 No Content
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePrivilegeHistory_ValidId_PrivilegeHistoryExists_ShouldReturnNoContent()
    {
        // Arrange
        var historyId = 1;
        _mockService.Setup(s => s.DeleteAsync(historyId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePrivilegeHistory(historyId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    /// <summary>
    /// EP2: Valid ID, PrivilegeHistory not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePrivilegeHistory_ValidId_PrivilegeHistoryNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var historyId = 999;
        _mockService.Setup(s => s.DeleteAsync(historyId))
            .ThrowsAsync(new ServicePrivilegeHistoryNotFoundException(historyId));

        // Act
        var result = await _controller.DeletePrivilegeHistory(historyId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var exception = Assert.IsType<HttpPrivilegeHistoryNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePrivilegeHistory_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var historyId = 1;
        _mockService.Setup(s => s.DeleteAsync(historyId))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeHistoryInternalServerException>(() => _controller.DeletePrivilegeHistory(historyId));
    }

    #endregion
}
