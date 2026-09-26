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
using presentation.dto.http.Flight;
using presentation.exceptions.http;
using presentation.exceptions.http.Flight;
using tests.config.attributes;
using tests.fixtures.mothers;

using ServiceFlightNotFoundException = core.exceptions.businesslogic.services.FlightNotFoundException;
using ServiceFlightValidationException = core.exceptions.businesslogic.services.FlightValidationException;
using HttpFlightNotFoundException = presentation.exceptions.http.Flight.FlightNotFoundException;
using HttpFlightValidationException = presentation.exceptions.http.Flight.FlightValidationException;
using FlightInternalServerException = presentation.exceptions.http.Flight.FlightInternalServerException;

namespace tests.presentation.controllers.unit;

/// <summary>
/// Unit tests for FlightHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetFlightById(int flightId):
/// - EP1: Valid ID, Flight exists (200 OK)
/// - EP2: Valid ID, Flight not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllFlights():
/// - EP1: Returns all flights (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreateFlight(CreateFlightDto):
/// - EP1: Valid flight, creation successful (201 Created)
/// - EP2: Invalid flight (400 Bad Request)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For UpdateFlight(int flightId, UpdateFlightDto):
/// - EP1: Valid flight, Flight exists (200 OK)
/// - EP2: Flight not found (404 Not Found)
/// - EP3: Invalid flight (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeleteFlight(int flightId):
/// - EP1: Valid ID, Flight exists (200 OK)
/// - EP2: Valid ID, Flight not found (404 Not Found)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// Total: 17 unit tests (all should pass)
/// </summary>
public class FlightHttpControllerUnitTests
{
    private readonly Mock<IFlightService> _mockService;
    private readonly Mock<ILogger<FlightHttpController>> _mockLogger;
    private readonly FlightHttpController _controller;

    public FlightHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<IFlightService>();
        _mockLogger = new Mock<ILogger<FlightHttpController>>();
        
        _controller = new FlightHttpController(_mockService.Object, _mockLogger.Object);
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var routeData = new RouteData();
        routeData.Values.Add("area", string.Empty);
        routeData.Values.Add("controller", "Flight");
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, routeData, new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetFlightById Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return 200 OK with Flight
    /// </summary>
    [Unit]
    public async Task GetFlightById_ValidId_FlightExists_ShouldReturnOk()
    {
        // Arrange
        var flight = FlightMother.CreateValidFlight();
        _mockService.Setup(s => s.GetByIdAsync(flight.Id)).ReturnsAsync(flight);

        // Act
        var result = await _controller.GetFlightById(flight.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<FlightDTO>(okResult.Value);
        
        Assert.Equal(flight.Id, dto.Id);
        _mockService.Verify(s => s.GetByIdAsync(flight.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight not found - should throw FlightNotFoundException
    /// </summary>
    [Unit]
    public async Task GetFlightById_FlightNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockService.Setup(s => s.GetByIdAsync(flightId))
            .ThrowsAsync(new ServiceFlightNotFoundException(flightId));

        // Act & Assert
        await Assert.ThrowsAsync<HttpFlightNotFoundException>(() => _controller.GetFlightById(flightId));
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetFlightById_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var flightId = 1;
        _mockService.Setup(s => s.GetByIdAsync(flightId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(() => _controller.GetFlightById(flightId));
    }

    #endregion

    #region GetAllFlights Tests

    /// <summary>
    /// EP1: Returns all flights - should return 200 OK with list
    /// </summary>
    [Unit]
    public async Task GetAllFlights_ReturnsAllFlights_ShouldReturnOk()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(flights);

        // Act
        var result = await _controller.GetAllFlights(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaginationResponse<FlightDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<PaginationResponse<FlightDTO>>(okResult.Value);
        
        Assert.Equal(5, response.Items.Count);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Unit]
    public async Task GetAllFlights_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var flights = new List<core.domain.Flight>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(flights);

        // Act
        var result = await _controller.GetAllFlights(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaginationResponse<FlightDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var response = Assert.IsType<PaginationResponse<FlightDTO>>(okResult.Value);
        
        Assert.Empty(response.Items);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task GetAllFlights_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(() => _controller.GetAllFlights(null, null));
    }

    #endregion

    #region CreateFlight Tests

    /// <summary>
    /// EP1: Valid flight, creation successful - should return 201 Created
    /// </summary>
    [Unit]
    public async Task CreateFlight_ValidFlight_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateFlightDTO
        {
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 50000
        };
        var createdFlight = FlightMother.CreateValidFlight();
        createdFlight.Id = 1;
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Flight>()))
            .ReturnsAsync(createdFlight);

        // Act
        var result = await _controller.CreateFlight(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var dto = Assert.IsType<FlightDTO>(createdResult.Value);
        
        Assert.Equal(createdFlight.Id, dto.Id);
    }

    /// <summary>
    /// EP2: Invalid flight - should throw FlightValidationException
    /// </summary>
    [Unit]
    public async Task CreateFlight_InvalidFlight_ShouldThrowValidationException()
    {
        // Arrange
        var createDto = new CreateFlightDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new ServiceFlightValidationException("Invalid flight"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpFlightValidationException>(() => _controller.CreateFlight(createDto));
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task CreateFlight_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var createDto = new CreateFlightDTO();
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(() => _controller.CreateFlight(createDto));
    }

    #endregion

    #region UpdateFlight Tests

    /// <summary>
    /// EP1: Valid flight, Flight exists - should return 200 OK
    /// </summary>
    [Unit]
    public async Task UpdateFlight_ValidFlight_FlightExists_ShouldReturnOk()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO
        {
            DateTime = DateTime.UtcNow.AddDays(2),
            FromAirportId = 3,
            ToAirportId = 4,
            Price = 60000
        };
        var updatedFlight = FlightMother.CreateValidFlight();
        updatedFlight.Id = flightId;
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Flight>()))
            .ReturnsAsync(updatedFlight);

        // Act
        var result = await _controller.UpdateFlight(flightId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<FlightDTO>(okResult.Value);
        
        Assert.Equal(flightId, dto.Id);
    }

    /// <summary>
    /// EP2: Flight not found - should throw FlightNotFoundException
    /// </summary>
    [Unit]
    public async Task UpdateFlight_FlightNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var flightId = 999;
        var updateDto = new UpdateFlightDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new ServiceFlightNotFoundException(flightId));

        // Act & Assert
        await Assert.ThrowsAsync<HttpFlightNotFoundException>(() => _controller.UpdateFlight(flightId, updateDto));
    }

    /// <summary>
    /// EP3: Invalid flight - should throw FlightValidationException
    /// </summary>
    [Unit]
    public async Task UpdateFlight_InvalidFlight_ShouldThrowValidationException()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new ServiceFlightValidationException("Invalid flight"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpFlightValidationException>(() => _controller.UpdateFlight(flightId, updateDto));
    }

    /// <summary>
    /// EP4: ID mismatch - should throw FlightInternalServerException (wraps validation)
    /// </summary>
    [Unit]
    public async Task UpdateFlight_IdMismatch_ShouldThrowInternalServerException()
    {
        // Arrange
        var routeFlightId = 1;
        var updateDto = new UpdateFlightDTO
        {
            DateTime = DateTime.UtcNow.AddDays(2),
            FromAirportId = 3,
            ToAirportId = 4,
            Price = 60000
        };
        var existingFlight = FlightMother.CreateValidFlight();
        existingFlight.Id = 5; // Different ID from route
        _mockService.Setup(s => s.GetByIdAsync(routeFlightId))
            .ReturnsAsync(existingFlight);

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(() => _controller.UpdateFlight(routeFlightId, updateDto));
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task UpdateFlight_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO();
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(() => _controller.UpdateFlight(flightId, updateDto));
    }

    #endregion

    #region DeleteFlight Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return 204 No Content
    /// </summary>
    [Unit]
    public async Task DeleteFlight_ValidId_FlightExists_ShouldReturnNoContent()
    {
        // Arrange
        var flightId = 1;
        _mockService.Setup(s => s.DeleteAsync(flightId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteFlight(flightId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        
        _mockService.Verify(s => s.DeleteAsync(flightId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight not found - should throw FlightNotFoundException
    /// </summary>
    [Unit]
    public async Task DeleteFlight_FlightNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var flightId = 999;
        _mockService.Setup(s => s.DeleteAsync(flightId))
            .ThrowsAsync(new ServiceFlightNotFoundException(flightId));

        // Act & Assert
        await Assert.ThrowsAsync<HttpFlightNotFoundException>(() => _controller.DeleteFlight(flightId));
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Unit]
    public async Task DeleteFlight_ServiceException_ShouldThrowInternalServerException()
    {
        // Arrange
        var flightId = 1;
        _mockService.Setup(s => s.DeleteAsync(flightId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(() => _controller.DeleteFlight(flightId));
    }

    #endregion
}
