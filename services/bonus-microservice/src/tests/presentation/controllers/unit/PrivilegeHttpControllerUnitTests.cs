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
using presentation.dto.http.Privilege;
using presentation.exceptions.http;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;
using CreatePrivilegeDTO = presentation.dto.http.Privilege.CreatePrivilegeDTO;
using UpdatePrivilegeDTO = presentation.dto.http.Privilege.UpdatePrivilegeDTO;

// Type aliases to avoid ambiguity
using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;
using ServicePrivilegeValidationException = core.exceptions.businesslogic.services.PrivilegeValidationException;
using ServicePrivilegeBusinessRuleViolationException = core.exceptions.businesslogic.services.PrivilegeBusinessRuleViolationException;
using HttpPrivilegeNotFoundException = presentation.exceptions.http.PrivilegeNotFoundException;
using HttpPrivilegeValidationException = presentation.exceptions.http.PrivilegeValidationException;
using HttpPrivilegeBusinessRuleViolationException = presentation.exceptions.http.PrivilegeBusinessRuleViolationException;
using PrivilegeInternalServerException = presentation.exceptions.http.PrivilegeInternalServerException;

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
/// - EP1: Valid ID, Privilege exists (204 No Content)
/// - EP2: Valid ID, Privilege not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreditBalance(int privilegeId, int amount, Guid ticketUid):
/// - EP1: Valid credit, successful (200 OK)
/// - EP2: Privilege not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For DebitBalance(int privilegeId, int amount, Guid ticketUid):
/// - EP1: Valid debit, successful (200 OK)
/// - EP2: Privilege not found (404 Not Found)
/// - EP3: Insufficient balance (409 Conflict)
/// - EP4: Service throws exception (500 Internal Server Error)
/// 
/// For GetMaxDebitAmount(int privilegeId):
/// - EP1: Valid ID, returns balance (200 OK)
/// - EP2: Privilege not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 24 unit tests (all should pass)
/// </summary>
public class PrivilegeHttpControllerUnitTests
{
    private readonly Mock<IPrivilegeService> _mockService;
    private readonly Mock<ILogger<PrivilegeHttpController>> _mockLogger;
    private readonly PrivilegeHttpController _controller;

    public PrivilegeHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<IPrivilegeService>();
        _mockLogger = new Mock<ILogger<PrivilegeHttpController>>();
        
        _controller = new PrivilegeHttpController(_mockService.Object, _mockLogger.Object);
        
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
    [Fact]
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
        Assert.Equal(privilege.Username, dto.Username);
        _mockService.Verify(s => s.GetByIdAsync(privilege.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPrivilegeById_ValidId_PrivilegeNotFound_ShouldReturnNotFound()
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
        
        Assert.Equal(404, dto.StatusCode);
        Assert.Equal("PRIVILEGE_NOT_FOUND", dto.ErrorCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPrivilegeById_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.GetByIdAsync(privilegeId))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.GetPrivilegeById(privilegeId));
    }

    #endregion

    #region GetAllPrivileges Tests

    /// <summary>
    /// EP1: Returns all privileges - should return 200 OK with list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPrivileges_ShouldReturnOkWithList()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(privileges);

        // Act
        var result = await _controller.GetAllPrivileges(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<PrivilegeDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<PrivilegeDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPrivileges_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var privileges = new List<core.domain.Privilege>();
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(privileges);

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
    [Fact]
    [Unit]
    public async Task GetAllPrivileges_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync())
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.GetAllPrivileges(null, null));
    }

    #endregion

    #region CreatePrivilege Tests

    /// <summary>
    /// EP1: Valid privilege, creation successful - should return 201 Created
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePrivilege_ValidPrivilege_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO("new.user", 0, 500);
        var createdPrivilege = PrivilegeMother.CreateValidPrivilege();
        createdPrivilege.Id = 10;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Privilege>()))
            .ReturnsAsync(createdPrivilege);

        // Act
        var result = await _controller.CreatePrivilege(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(201, createdResult.StatusCode);
        
        var dto = Assert.IsType<PrivilegeDTO>(createdResult.Value);
        Assert.Equal(createdPrivilege.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid privilege - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePrivilege_InvalidPrivilege_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO("", 0, 500); // Empty username
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.CreatePrivilege(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePrivilege_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO("new.user", 0, 500);
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.CreatePrivilege(createDto));
    }

    #endregion

    #region UpdatePrivilege Tests

    /// <summary>
    /// EP1: Valid privilege, Privilege exists - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilege_ValidPrivilege_PrivilegeExists_ShouldReturnOk()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var updateDto = new UpdatePrivilegeDTO(privilege.Id, "updated.user", 1, 2000);
        _mockService.Setup(s => s.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Privilege>()))
            .ReturnsAsync(privilege);

        // Act
        var result = await _controller.UpdatePrivilege(privilege.Id, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeDTO>(okResult.Value);
        
        Assert.Equal(privilege.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Privilege not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilege_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        var updateDto = new UpdatePrivilegeDTO(privilegeId, "updated.user", 1, 2000);
        _mockService.Setup(s => s.GetByIdAsync(privilegeId))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.UpdatePrivilege(privilegeId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Invalid privilege - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilege_InvalidPrivilege_ShouldReturnBadRequest()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var updateDto = new UpdatePrivilegeDTO(privilege.Id, "", 1, 2000); // Empty username
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.UpdatePrivilege(privilege.Id, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, exception.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilege_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var updateDto = new UpdatePrivilegeDTO(999, "updated.user", 1, 2000); // Different ID

        // Act
        var result = await _controller.UpdatePrivilege(privilege.Id, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, exception.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePrivilege_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var updateDto = new UpdatePrivilegeDTO(privilege.Id, "updated.user", 1, 2000);
        _mockService.Setup(s => s.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Privilege>()))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.UpdatePrivilege(privilege.Id, updateDto));
    }

    #endregion

    #region DeletePrivilege Tests

    /// <summary>
    /// EP1: Valid ID, Privilege exists - should return 204 No Content
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePrivilege_ValidId_PrivilegeExists_ShouldReturnNoContent()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.DeleteAsync(privilegeId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePrivilege(privilegeId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePrivilege_ValidId_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        _mockService.Setup(s => s.DeleteAsync(privilegeId))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.DeletePrivilege(privilegeId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var exception = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePrivilege_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.DeleteAsync(privilegeId))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.DeletePrivilege(privilegeId));
    }

    #endregion

    #region CreditBalance Tests

    /// <summary>
    /// EP1: Valid credit, successful - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreditBalance_ValidCredit_ShouldReturnOk()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var amount = 500;
        var ticketUid = Guid.NewGuid();
        privilege.Balance += amount;
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.CreditBalanceAsync(privilege.Id, amount, ticketUid))
            .ReturnsAsync(privilege);

        // Act
        var result = await _controller.CreditBalance(privilege.Id, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeDTO>(okResult.Value);
        
        Assert.Equal(privilege.Id, dto.Id);
        Assert.Equal(privilege.Balance, dto.Balance);
    }

    /// <summary>
    /// EP2: Privilege not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreditBalance_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        var amount = 500;
        var ticketUid = Guid.NewGuid();
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.CreditBalanceAsync(privilegeId, amount, ticketUid))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.CreditBalance(privilegeId, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreditBalance_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        var amount = 500;
        var ticketUid = Guid.NewGuid();
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.CreditBalanceAsync(privilegeId, amount, ticketUid))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.CreditBalance(privilegeId, request));
    }

    #endregion

    #region DebitBalance Tests

    /// <summary>
    /// EP1: Valid debit, successful - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalance_ValidDebit_ShouldReturnOk()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var amount = 200;
        var ticketUid = Guid.NewGuid();
        privilege.Balance -= amount;
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.DebitBalanceAsync(privilege.Id, amount, ticketUid))
            .ReturnsAsync(privilege);

        // Act
        var result = await _controller.DebitBalance(privilege.Id, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PrivilegeDTO>(okResult.Value);
        
        Assert.Equal(privilege.Id, dto.Id);
        Assert.Equal(privilege.Balance, dto.Balance);
    }

    /// <summary>
    /// EP2: Privilege not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalance_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        var amount = 200;
        var ticketUid = Guid.NewGuid();
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.DebitBalanceAsync(privilegeId, amount, ticketUid))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.DebitBalance(privilegeId, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Insufficient balance - should return 409 Conflict
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalance_InsufficientBalance_ShouldReturnConflict()
    {
        // Arrange
        var privilege = PrivilegeMother.CreatePrivilegeWithZeroBalance();
        var amount = 100;
        var ticketUid = Guid.NewGuid();
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.DebitBalanceAsync(privilege.Id, amount, ticketUid))
            .ThrowsAsync(new ServicePrivilegeBusinessRuleViolationException("INSUFFICIENT_BALANCE", "Insufficient balance"));

        // Act
        var result = await _controller.DebitBalance(privilege.Id, request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PrivilegeDTO>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeBusinessRuleViolationException>(conflictResult.Value);
        
        Assert.Equal(409, exception.StatusCode);
    }

    /// <summary>
    /// EP4: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalance_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        var amount = 200;
        var ticketUid = Guid.NewGuid();
        var request = new CreditDebitBonusRequest { Amount = amount, TicketUid = ticketUid };
        _mockService.Setup(s => s.DebitBalanceAsync(privilegeId, amount, ticketUid))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.DebitBalance(privilegeId, request));
    }

    #endregion

    #region GetMaxDebitAmount Tests

    /// <summary>
    /// EP1: Valid ID, returns balance - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetMaxDebitAmount_ValidId_ShouldReturnOk()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var maxAmount = privilege.Balance;
        _mockService.Setup(s => s.GetMaxDebitAmountAsync(privilege.Id))
            .ReturnsAsync(maxAmount);

        // Act
        var result = await _controller.GetMaxDebitAmount(privilege.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<int>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var amount = Assert.IsType<int>(okResult.Value);
        
        Assert.Equal(maxAmount, amount);
    }

    /// <summary>
    /// EP2: Privilege not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetMaxDebitAmount_PrivilegeNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var privilegeId = 999;
        _mockService.Setup(s => s.GetMaxDebitAmountAsync(privilegeId))
            .ThrowsAsync(new ServicePrivilegeNotFoundException(privilegeId));

        // Act
        var result = await _controller.GetMaxDebitAmount(privilegeId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<int>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var exception = Assert.IsType<HttpPrivilegeNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, exception.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetMaxDebitAmount_ServiceThrowsException_ShouldThrowInternalServerException()
    {
        // Arrange
        var privilegeId = 1;
        _mockService.Setup(s => s.GetMaxDebitAmountAsync(privilegeId))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<PrivilegeInternalServerException>(() => _controller.GetMaxDebitAmount(privilegeId));
    }

    #endregion
}
