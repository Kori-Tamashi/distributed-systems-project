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
using presentation.dto.http.Airport;
using presentation.exceptions.http;
using presentation.exceptions.http.Airport;
using tests.config.attributes;
using tests.fixtures.mothers;

using ServiceAirportNotFoundException = core.exceptions.businesslogic.services.AirportNotFoundException;
using ServiceAirportValidationException = core.exceptions.businesslogic.services.AirportValidationException;
using HttpAirportNotFoundException = presentation.exceptions.http.Airport.AirportNotFoundException;
using HttpAirportValidationException = presentation.exceptions.http.Airport.AirportValidationException;
using AirportInternalServerException = presentation.exceptions.http.Airport.AirportInternalServerException;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for AirportHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetAirportById(int airportId):
/// - EP1: Valid ID, Airport exists (200 OK)
/// - EP2: Valid ID, Airport not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllAirports():
/// - EP1: Returns all airports (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreateAirport(CreateAirportDto):
/// - EP1: Valid airport, creation successful (201 Created)
/// - EP2: Invalid airport (400 Bad Request)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For UpdateAirport(int airportId, UpdateAirportDto):
/// - EP1: Valid airport, Airport exists (200 OK)
/// - EP2: Airport not found (404 Not Found)
/// - EP3: Invalid airport (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeleteAirport(int airportId):
/// - EP1: Valid ID, Airport exists (200 OK)
/// - EP2: Valid ID, Airport not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 17 unit tests (all should pass)
/// </summary>
public class AirportHttpControllerUnitTests
{
    private readonly Mock<IAirportService> _mockService;
    private readonly Mock<ILogger<AirportHttpController>> _mockLogger;
    private readonly AirportHttpController _controller;

    public AirportHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<IAirportService>();
        _mockLogger = new Mock<ILogger<AirportHttpController>>();
        
        _controller = new AirportHttpController(_mockService.Object, _mockLogger.Object);
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "Airport");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetAirportById Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists - should return 200 OK with Airport
    /// </summary>
    [Unit]
    public async Task GetAirportById_ValidId_AirportExists_ShouldReturnOk()
    {
        // Arrange
        var airport = AirportMother.CreateValidAirport();
        _mockService.Setup(s => s.GetByIdAsync(airport.Id)).ReturnsAsync(airport);

        // Act
        var result = await _controller.GetAirportById(airport.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<AirportDTO>(okResult.Value);
        
        Assert.Equal(airport.Id, dto.Id);
        _mockService.Verify(s => s.GetByIdAsync(airport.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task GetAirportById_AirportNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var airportId = 999;
        _mockService.Setup(s => s.GetByIdAsync(airportId))
            .ThrowsAsync(new ServiceAirportNotFoundException(airportId));

        // Act
        var result = await _controller.GetAirportById(airportId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpAirportNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(airportId, dto.AirportId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetAirportById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var airportId = 1;
        _mockService.Setup(s => s.GetByIdAsync(airportId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<AirportInternalServerException>(() => _controller.GetAirportById(airportId));
    }

    #endregion

    #region GetAllAirports Tests

    /// <summary>
    /// EP1: Returns all airports - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetAllAirports_ReturnsAllAirports_ShouldReturnOk()
    {
        // Arrange
        var airports = AirportMother.CreateAirportList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(airports);

        // Act
        var result = await _controller.GetAllAirports(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<AirportDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<AirportDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Unit]
    public async Task GetAllAirports_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var airports = new List<core.domain.Airport>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(airports);

        // Act
        var result = await _controller.GetAllAirports(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<AirportDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<AirportDTO>>(okResult.Value);
        
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetAllAirports_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<AirportInternalServerException>(() => _controller.GetAllAirports(null, null));
    }

    #endregion

    #region CreateAirport Tests

    /// <summary>
    /// EP1: Valid airport, creation successful - should return 201 Created
    /// </summary>
    [Unit]
    public async Task CreateAirport_ValidAirport_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateAirportDTO
        {
            Name = "Test Airport",
            City = "Moscow",
            Country = "Russia"
        };
        var createdAirport = AirportMother.CreateValidAirport();
        createdAirport.Id = 1;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Airport>()))
            .ReturnsAsync(createdAirport);

        // Act
        var result = await _controller.CreateAirport(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var dto = Assert.IsType<AirportDTO>(createdResult.Value);
        
        Assert.Equal(createdAirport.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid airport - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task CreateAirport_InvalidAirport_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateAirportDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Airport>()))
            .ThrowsAsync(new ServiceAirportValidationException("Invalid airport"));

        // Act
        var result = await _controller.CreateAirport(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpAirportValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task CreateAirport_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreateAirportDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Airport>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<AirportInternalServerException>(() => _controller.CreateAirport(createDto));
    }

    #endregion

    #region UpdateAirport Tests

    /// <summary>
    /// EP1: Valid airport, Airport exists - should return 200 OK
    /// </summary>
    [Unit]
    public async Task UpdateAirport_ValidAirport_AirportExists_ShouldReturnOk()
    {
        // Arrange
        var airportId = 1;
        var updateDto = new UpdateAirportDTO
        {
            Name = "Updated Airport",
            City = "St. Petersburg",
            Country = "Russia"
        };
        var updatedAirport = AirportMother.CreateValidAirport();
        updatedAirport.Id = airportId;
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Airport>()))
            .ReturnsAsync(updatedAirport);

        // Act
        var result = await _controller.UpdateAirport(airportId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<AirportDTO>(okResult.Value);
        
        Assert.Equal(airportId, dto.Id);
    }

    /// <summary>
    /// EP2: Airport not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task UpdateAirport_AirportNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var airportId = 999;
        var updateDto = new UpdateAirportDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Airport>()))
            .ThrowsAsync(new ServiceAirportNotFoundException(airportId));

        // Act
        var result = await _controller.UpdateAirport(airportId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpAirportNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(airportId, dto.AirportId);
    }

    /// <summary>
    /// EP3: Invalid airport - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdateAirport_InvalidAirport_ShouldReturnBadRequest()
    {
        // Arrange
        var airportId = 1;
        var updateDto = new UpdateAirportDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Airport>()))
            .ThrowsAsync(new ServiceAirportValidationException("Invalid airport"));

        // Act
        var result = await _controller.UpdateAirport(airportId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpAirportValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Unit]
    public async Task UpdateAirport_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var routeAirportId = 1;
        var updateDto = new UpdateAirportDTO
        {
            Name = "Updated Airport",
            City = "St. Petersburg",
            Country = "Russia"
        };
        var existingAirport = AirportMother.CreateValidAirport();
        existingAirport.Id = 5; // Different ID from route
        _mockService.Setup(s => s.GetByIdAsync(routeAirportId))
            .ReturnsAsync(existingAirport);

        // Act
        var result = await _controller.UpdateAirport(routeAirportId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<AirportDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpAirportValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task UpdateAirport_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var airportId = 1;
        var updateDto = new UpdateAirportDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Airport>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<AirportInternalServerException>(() => _controller.UpdateAirport(airportId, updateDto));
    }

    #endregion

    #region DeleteAirport Tests

    /// <summary>
    /// EP1: Valid ID, Airport exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task DeleteAirport_ValidId_AirportExists_ShouldReturnNoContent()
    {
        // Arrange
        var airportId = 1;
        _mockService.Setup(s => s.DeleteAsync(airportId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAirport(airportId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockService.Verify(s => s.DeleteAsync(airportId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Airport not found - should return 404 Not Found
    /// </summary>
    [Unit]
    public async Task DeleteAirport_AirportNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var airportId = 999;
        _mockService.Setup(s => s.DeleteAsync(airportId))
            .ThrowsAsync(new ServiceAirportNotFoundException(airportId));

        // Act
        var result = await _controller.DeleteAirport(airportId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<HttpAirportNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(airportId, dto.AirportId);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task DeleteAirport_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var airportId = 1;
        _mockService.Setup(s => s.DeleteAsync(airportId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<AirportInternalServerException>(() => _controller.DeleteAirport(airportId));
    }

    #endregion
}
