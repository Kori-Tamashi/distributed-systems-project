using core.domain;
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
using tests.config.attributes;
using tests.fixtures.mothers;
using CreateFlightDTO = presentation.dto.http.CreateFlightDTO;
using UpdateFlightDTO = presentation.dto.http.Flight.UpdateFlightDTO;

// Type aliases to avoid ambiguity
using ServiceFlightNotFoundException = core.exceptions.businesslogic.services.FlightNotFoundException;
using ServiceFlightValidationException = core.exceptions.businesslogic.services.FlightValidationException;
using ServiceFlightBusinessRuleViolationException = core.exceptions.businesslogic.services.FlightBusinessRuleViolationException;
using HttpFlightNotFoundException = presentation.exceptions.http.FlightNotFoundException;
using HttpFlightValidationException = presentation.exceptions.http.FlightValidationException;
using HttpFlightBusinessRuleViolationException = presentation.exceptions.http.FlightBusinessRuleViolationException;
using FlightInternalServerException = presentation.exceptions.http.FlightInternalServerException;

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
/// - EP3: Airport not found (404 Not Found)
/// - EP4: Service throws exception (500 Internal Server Error)
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
    [Fact]
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
        Assert.Equal(flight.FlightNumber, dto.FlightNumber);
        _mockService.Verify(s => s.GetByIdAsync(flight.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetFlightById_ValidId_FlightNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var flightId = 999;
        _mockService.Setup(s => s.GetByIdAsync(flightId))
            .ThrowsAsync(new ServiceFlightNotFoundException(flightId));

        // Act
        var result = await _controller.GetFlightById(flightId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpFlightNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(flightId, dto.FlightId);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetFlightById_ServiceError_ShouldThrowException()
    {
        // Arrange
        var flightId = 1;
        _mockService.Setup(s => s.GetByIdAsync(flightId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(
            () => _controller.GetFlightById(flightId)
        );
    }

    #endregion

    #region GetAllFlights Tests

    /// <summary>
    /// EP1: Returns all flights - should return 200 OK with list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllFlights_NoFilter_ShouldReturnOkWithList()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(flights);

        // Act
        var result = await _controller.GetAllFlights(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<FlightDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<FlightDTO>>(okResult.Value);
        
        Assert.Equal(5, dtos.Count);
        _mockService.Verify(s => s.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllFlights_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var flights = new List<core.domain.Flight>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(flights);

        // Act
        var result = await _controller.GetAllFlights(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<FlightDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<FlightDTO>>(okResult.Value);
        
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllFlights_ServiceError_ShouldThrowException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(
            () => _controller.GetAllFlights(null, null)
        );
    }

    /// <summary>
    /// EP4: With pagination - should apply pagination correctly
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllFlights_WithPagination_ShouldApplyPagination()
    {
        // Arrange
        var flights = FlightMother.CreateFlightList(100);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(flights);

        // Act
        var result = await _controller.GetAllFlights(page: 2, pageSize: 10);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<FlightDTO>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsType<List<FlightDTO>>(okResult.Value);
        
        Assert.Equal(10, dtos.Count);
    }

    #endregion

    #region CreateFlight Tests



    /// <summary>
    /// EP2: Valid flight - should return 201 Created
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateFlight_ValidFlight_ShouldReturnCreatedWithLocation()
    {
        // Arrange
        var createDto = new CreateFlightDTO
        {
            FlightNumber = "SU1234",
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 15000
        };
        
        var flightDomain = new Flight
        {
            Id = 1,
            FlightNumber = createDto.FlightNumber,
            FlightUid = createDto.FlightUid,
            DateTime = createDto.DateTime,
            FromAirportId = createDto.FromAirportId,
            ToAirportId = createDto.ToAirportId,
            Price = createDto.Price
        };
        
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Flight>())).ReturnsAsync(flightDomain);
        
        // Setup mock UrlHelper to avoid routing errors
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns($"/api/v1/flights/{flightDomain.Id}");
        
        // Update controller with mocked UrlHelper
        _controller.Url = urlHelperMock.Object;

        // Act
        var result = await _controller.CreateFlight(createDto);

        // Assert
        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetFlightById", actionResult.ActionName);
        Assert.Equal(flightDomain.Id, ((RouteValueDictionary)actionResult.RouteValues)["flightId"]);
    }

    /// <summary>
    /// EP3: Invalid flight (empty name) - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateFlight_InvalidFlight_EmptyFlightNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateFlightDTO
        {
            FlightNumber = "", // Invalid
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 15000
        };
        
        _controller.ModelState.AddModelError("FlightNumber", "FlightNumber is required");

        // Act
        var result = await _controller.CreateFlight(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpFlightValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Airport not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateFlight_AirportNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var createDto = new CreateFlightDTO
        {
            FlightNumber = "SU1234",
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 999, // Non-existent
            ToAirportId = 2,
            Price = 15000
        };
        
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new AirportNotFoundException(999));

        // Act
        var result = await _controller.CreateFlight(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<AirportNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(404, dto.StatusCode);
        Assert.Equal(999, dto.AirportId);
    }

    /// <summary>
    /// EP4: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateFlight_ServiceError_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateFlightDTO
        {
            FlightNumber = "SU1234",
            FlightUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow.AddDays(1),
            FromAirportId = 1,
            ToAirportId = 2,
            Price = 15000
        };
        
        _mockService.Setup(s => s.CreateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(
            () => _controller.CreateFlight(createDto)
        );
    }

    #endregion

    #region UpdateFlight Tests

    /// <summary>
    /// EP1: Valid flight, Flight exists - should return 200 OK with updated Flight
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateFlight_ValidFlight_FlightExists_ShouldReturnOk()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO(flightId)
        {
            FlightNumber = "Updated123",
            Price = 25000
        };
        
        var existingFlight = FlightMother.CreateValidFlight();
        existingFlight.Id = flightId;
        
        var updatedFlight = FlightMother.CreateValidFlight();
        updatedFlight.Id = flightId;
        updatedFlight.FlightNumber = updateDto.FlightNumber;
        updatedFlight.Price = updateDto.Price ?? existingFlight.Price;
        
        _mockService.Setup(s => s.GetByIdAsync(flightId)).ReturnsAsync(existingFlight);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Flight>())).ReturnsAsync(updatedFlight);

        // Act
        var result = await _controller.UpdateFlight(flightId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<FlightDTO>(okResult.Value);
        
        Assert.Equal(flightId, dto.Id);
        Assert.Equal(updateDto.FlightNumber, dto.FlightNumber);
        _mockService.Verify(s => s.UpdateAsync(It.IsAny<core.domain.Flight>()), Times.Once);
    }

    /// <summary>
    /// EP2: Flight not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateFlight_FlightNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var flightId = 999;
        var updateDto = new UpdateFlightDTO(flightId)
        {
            FlightNumber = "Updated123"
        };
        
        _mockService.Setup(s => s.GetByIdAsync(flightId))
            .ThrowsAsync(new ServiceFlightNotFoundException(flightId));

        // Act
        var result = await _controller.UpdateFlight(flightId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpFlightNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(flightId, dto.FlightId);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Invalid flight (empty name) - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateFlight_InvalidFlight_EmptyFlightNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO(flightId)
        {
            FlightNumber = "" // Invalid
        };
        
        _controller.ModelState.AddModelError("FlightNumber", "FlightNumber is required");

        // Act
        var result = await _controller.UpdateFlight(flightId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpFlightValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateFlight_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO(999) // Different from route ID
        {
            FlightNumber = "Updated123"
        };

        // Act
        var result = await _controller.UpdateFlight(flightId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<FlightDTO>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<HttpFlightValidationException>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateFlight_ServiceError_ShouldThrowException()
    {
        // Arrange
        var flightId = 1;
        var updateDto = new UpdateFlightDTO(flightId)
        {
            FlightNumber = "Updated123"
        };
        
        _mockService.Setup(s => s.GetByIdAsync(flightId)).ReturnsAsync(FlightMother.CreateValidFlight());
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<core.domain.Flight>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(
            () => _controller.UpdateFlight(flightId, updateDto)
        );
    }

    #endregion

    #region DeleteFlight Tests

    /// <summary>
    /// EP1: Valid ID, Flight exists - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteFlight_ValidId_FlightExists_ShouldReturnOk()
    {
        // Arrange
        var flightId = 1;
        _mockService.Setup(s => s.DeleteAsync(flightId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteFlight(flightId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        _mockService.Verify(s => s.DeleteAsync(flightId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Flight not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteFlight_ValidId_FlightNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var flightId = 999;
        _mockService.Setup(s => s.DeleteAsync(flightId))
            .ThrowsAsync(new ServiceFlightNotFoundException(flightId));

        // Act
        var result = await _controller.DeleteFlight(flightId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<HttpFlightNotFoundException>(notFoundResult.Value);
        
        Assert.Equal(flightId, dto.FlightId);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteFlight_ServiceError_ShouldThrowException()
    {
        // Arrange
        var flightId = 1;
        _mockService.Setup(s => s.DeleteAsync(flightId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FlightInternalServerException>(
            () => _controller.DeleteFlight(flightId)
        );
    }

    #endregion
}
