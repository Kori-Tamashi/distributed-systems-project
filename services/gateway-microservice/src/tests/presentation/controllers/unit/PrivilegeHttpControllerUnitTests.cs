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
using presentation.dto.http.Privilege;
using presentation.exceptions.http;
using presentation.exceptions.http.Privilege;
using tests.config.attributes;
using tests.fixtures.mothers;

using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;
using ServicePrivilegeValidationException = core.exceptions.businesslogic.services.PrivilegeValidationException;
using HttpPrivilegeNotFoundException = presentation.exceptions.http.Privilege.PrivilegeNotFoundException;
using HttpPrivilegeValidationException = presentation.exceptions.http.Privilege.PrivilegeValidationException;
using PrivilegeInternalServerException = presentation.exceptions.http.Privilege.PrivilegeInternalServerException;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for PrivilegeHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetPrivilegeById(int privilegeId):
/// - EP1: Valid ID, Privilege exists (200 OK)
/// - EP2: Valid ID, Privilege not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllPrivileges():
/// - EP1: Returns all privileges (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreatePrivilege(CreatePrivilegeDto):
/// - EP1: Valid privilege, creation successful (201 Created)
/// - EP2: Invalid privilege (400 Bad Request)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For UpdatePrivilege(int privilegeId, UpdatePrivilegeDto):
/// - EP1: Valid privilege, Privilege exists (200 OK)
/// - EP2: Privilege not found (404 Not Found)
/// - EP3: Invalid privilege (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeletePrivilege(int privilegeId):
/// - EP1: Valid ID, Privilege exists (200 OK)
/// - EP2: Valid ID, Privilege not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 17 unit tests (all should pass)
/// </summary>
public class PrivilegeHttpControllerUnitTests
{
    private readonly Mock<IPrivilegeService> _mockService;
    private readonly Mock<IPrivilegeHistoryService> _mockPrivilegeHistoryService;
    private readonly Mock<ILogger<PrivilegeHttpController>> _mockLogger;
    private readonly PrivilegeHttpController _controller;

    public PrivilegeHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<IPrivilegeService>();
        _mockPrivilegeHistoryService = new Mock<IPrivilegeHistoryService>();
        _mockLogger = new Mock<ILogger<PrivilegeHttpController>>();
        
        _controller = new PrivilegeHttpController(_mockService.Object, _mockPrivilegeHistoryService.Object, _mockLogger.Object);
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "Privilege");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetPrivilegeById Tests

    /// <summary>
    /// EP1: Valid ID, Privilege exists - should return 200 OK with Privilege
    /// </summary>
    [Unit]
    public async Task GetPrivilegeById_ValidId_PrivilegeExists_ShouldReturnOk()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockService.Setup(s => s.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act
        var result = await _controller.GetPrivilegeById(privilege.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeDTO>(okResult.Value);
        
        Assert.Equal(privilege.Id, dto.Id);
        _mockService.Verify(s => s.GetByIdAsync(privilege.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetPrivilegeById_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        _mockService.Setup(s => s.GetByIdAsync(privilegeId))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.GetPrivilegeById(privilegeId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(privilegeId, dto.PrivilegeId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetPrivilegeById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.GetByIdAsync(privilegeId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.GetPrivilegeById(privilegeId));
    }

    #endregion

    #region GetAllPrivileges Tests

    /// <summary>
    /// EP1: Returns all privileges - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetAllPrivileges_ReturnsAllPrivileges_ShouldReturnOk()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(privileges);

        // Act
        var result = await _controller.GetAllPrivileges(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PrivilegeDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<PrivilegeDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Unit]
    public async Task GetAllPrivileges_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var privileges = new List<core.domain.Privilege>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(privileges);

        // Act
        var result = await _controller.GetAllPrivileges(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PrivilegeDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<PrivilegeDTO>>(okResult.Value);
        
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetAllPrivileges_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.GetAllPrivileges(null, null));
    }

    #endregion

    #region CreatePrivilege Tests

    /// <summary>
    /// EP1: Valid privilege, creation successful - should return 201 Created
    /// </summary>
    [Unit]
    public async Task CreatePrivilege_ValidPrivilege_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO
        {
            Username = "testuser",
            Status = 0,
            Balance = 5000
        };
        var createdPrivilege = PrivilegeMother.CreateValidPrivilege();
        createdPrivilege.Id = 1;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Privilege>()))
            .ReturnsAsync(createdPrivilege);

        // Act
        var result = await _controller.CreatePrivilege(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeDTO>(createdResult.Value);
        
        Assert.Equal(createdPrivilege.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid privilege - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task CreatePrivilege_InvalidPrivilege_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new ServicePrivilegeValidationException("Invalid privilege"));

        // Act
        var result = await _controller.CreatePrivilege(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpPrivilegeValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task CreatePrivilege_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.CreatePrivilege(createDto));
    }

    #endregion

    #region UpdatePrivilege Tests

    /// <summary>
    /// EP1: Valid privilege, Privilege exists - should return 200 OK
    /// </summary>
    [Unit]
    public async Task UpdatePrivilege_ValidPrivilege_PrivilegeExists_ShouldReturnOk()
    {
        // Arrange
        var privilegeId = 1;
        var updateDto = new UpdatePrivilegeDTO
        {
            Username = "updateduser",
            Status = 1,
            Balance = 10000
        };
        var updatedPrivilege = PrivilegeMother.CreateValidPrivilege();
        updatedPrivilege.Id = privilegeId;
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Privilege>()))
            .ReturnsAsync(updatedPrivilege);

        // Act
        var result = await _controller.UpdatePrivilege(privilegeId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeDTO>(okResult.Value);
        
        Assert.Equal(privilegeId, dto.Id);
    }

    /// <summary>
    /// EP2: Privilege not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task UpdatePrivilege_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        var updateDto = new UpdatePrivilegeDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.UpdatePrivilege(privilegeId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(privilegeId, dto.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid privilege - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdatePrivilege_InvalidPrivilege_ShouldReturnBadRequest()
    {
        // Arrange
        var privilegeId = 1;
        var updateDto = new UpdatePrivilegeDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new ServicePrivilegeValidationException("Invalid privilege"));

        // Act
        var result = await _controller.UpdatePrivilege(privilegeId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpPrivilegeValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdatePrivilege_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var routePrivilegeId = 1;
        var updateDto = new UpdatePrivilegeDTO
        {
            Username = "updateduser",
            Status = 1,
            Balance = 10000
        };
        var existingPrivilege = PrivilegeMother.CreateValidPrivilege();
        existingPrivilege.Id = 5; // Different ID from route
        _mockService.Setup(s => s.GetByIdAsync(routePrivilegeId))
            .ReturnsAsync(existingPrivilege);

        // Act
        var result = await _controller.UpdatePrivilege(routePrivilegeId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpPrivilegeValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task UpdatePrivilege_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        var updateDto = new UpdatePrivilegeDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.UpdatePrivilege(privilegeId, updateDto));
    }

    #endregion

    #region DeletePrivilege Tests

    /// <summary>
    /// EP1: Valid ID, Privilege exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task DeletePrivilege_ValidId_PrivilegeExists_ShouldReturnNoContent()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.DeleteAsync(privilegeId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePrivilege(privilegeId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockService.Verify(s => s.DeleteAsync(privilegeId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task DeletePrivilege_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        _mockService.Setup(s => s.DeleteAsync(privilegeId))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.DeletePrivilege(privilegeId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(privilegeId, dto.PrivilegeId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task DeletePrivilege_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.DeleteAsync(privilegeId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.DeletePrivilege(privilegeId));
    }

    #endregion
}
