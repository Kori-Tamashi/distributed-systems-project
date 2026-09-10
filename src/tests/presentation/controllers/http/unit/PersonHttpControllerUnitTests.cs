using core.domain;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using presentation.controllers.http;
using presentation.converters;
using presentation.dto.http;
using presentation.exceptions.http;
using tests.config.attributes;
using tests.fixtures.mothers;

namespace tests.presentation.controllers.http.unit;

/// <summary>
/// Unit tests for PersonHttpController
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetPersonById(int personId):
/// - EP1: Valid ID, Person exists (200 OK)
/// - EP2: Valid ID, Person not found (404 Not Found)
/// - EP3: Invalid ID (<= 0) (400 Bad Request)
/// - EP4: Service throws exception (500 Internal Server Error)
/// 
/// For GetAllPersons():
/// - EP1: Returns all persons (200 OK)
/// - EP2: Returns empty list (200 OK)
/// - EP3: Service throws exception (500 Internal Server Error)
/// 
/// For CreatePerson(CreatePersonHttpDto):
/// - EP1: Valid person, creation successful (201 Created)
/// - EP2: Invalid person (400 Bad Request)
/// - EP3: Person already exists (409 Conflict)
/// - EP4: Service throws exception (500 Internal Server Error)
/// 
/// For UpdatePerson(int personId, UpdatePersonHttpDto):
/// - EP1: Valid person, Person exists (200 OK)
/// - EP2: Person not found (404 Not Found)
/// - EP3: Invalid person (400 Bad Request)
/// - EP4: ID mismatch (400 Bad Request)
/// - EP5: Service throws exception (500 Internal Server Error)
/// 
/// For DeletePerson(int personId):
/// - EP1: Valid ID, Person exists (200 OK)
/// - EP2: Valid ID, Person not found (404 Not Found)
/// - EP3: Invalid ID (<= 0) (400 Bad Request)
/// - EP4: Service throws exception (500 Internal Server Error)
/// 
/// Total: 22 unit tests (all should pass)
/// </summary>
public class PersonHttpControllerUnitTests
{
    private readonly Mock<IPersonService> _mockService;
    private readonly Mock<ILogger<PersonHttpController>> _mockLogger;
    private readonly PersonHttpController _controller;

    public PersonHttpControllerUnitTests()
    {
        // Arrange - Setup mocks
        _mockService = new Mock<IPersonService>();
        _mockLogger = new Mock<ILogger<PersonHttpController>>();
        
        _controller = new PersonHttpController(_mockService.Object, _mockLogger.Object);
        
        // Setup URL helper for Location header
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var urlHelper = new UrlHelper(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));
        _controller.Url = urlHelper;
    }

    #region GetPersonById Tests

    /// <summary>
    /// EP1: Valid ID, Person exists - should return 200 OK with Person
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPersonById_ValidId_PersonExists_ShouldReturnOk()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        _mockService.Setup(s => s.GetByIdAsync(person.Id)).ReturnsAsync(person);

        // Act
        var result = await _controller.GetPersonById(person.Id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonHttpDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonHttpDto>(okResult.Value);
        
        Assert.Equal(person.Id, dto.Id);
        Assert.Equal(person.Name, dto.Name);
        _mockService.Verify(s => s.GetByIdAsync(person.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Person not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPersonById_ValidId_PersonNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 999;
        _mockService.Setup(s => s.GetByIdAsync(personId))
            .ThrowsAsync(new PersonNotFoundException(personId));

        // Act
        var result = await _controller.GetPersonById(personId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonHttpDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonNotFoundHttpDto>(notFoundResult.Value);
        
        Assert.Equal(personId, dto.PersonId);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetPersonById_ServiceError_ShouldThrowException()
    {
        // Arrange
        var personId = 1;
        _mockService.Setup(s => s.GetByIdAsync(personId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPersonServiceException>(
            () => _controller.GetPersonById(personId)
        );
    }

    #endregion

    #region GetAllPersons Tests

    /// <summary>
    /// EP1: Returns all persons - should return 200 OK with list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPersons_NoFilter_ShouldReturnOkWithList()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(persons);

        // Act
        var result = await _controller.GetAllPersons(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonListHttpDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var listDto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        
        Assert.Equal(5, listDto.Items.Count);
        Assert.Equal(5, listDto.TotalCount);
        _mockService.Verify(s => s.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: Returns empty list - should return 200 OK with empty list
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPersons_EmptyList_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var persons = new List<Person>();
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(persons);

        // Act
        var result = await _controller.GetAllPersons(null, null);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonListHttpDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var listDto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        
        Assert.Empty(listDto.Items);
        Assert.Equal(0, listDto.TotalCount);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPersons_ServiceError_ShouldThrowException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPersonServiceException>(
            () => _controller.GetAllPersons(null, null)
        );
    }

    /// <summary>
    /// EP4: With pagination - should apply pagination correctly
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllPersons_WithPagination_ShouldApplyPagination()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(100);
        _mockService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(persons);

        // Act
        var result = await _controller.GetAllPersons(page: 2, pageSize: 10);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonListHttpDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var listDto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        
        Assert.Equal(10, listDto.Items.Count);
        Assert.Equal(100, listDto.TotalCount);
        Assert.Equal(2, listDto.Page);
        Assert.Equal(10, listDto.PageSize);
    }

    #endregion

    #region CreatePerson Tests

    /// <summary>
    /// EP1: Valid person, creation successful - should return 201 Created with Location header
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePerson_ValidPerson_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto
        {
            Name = "John Doe",
            Age = 30,
            Address = "123 Main St",
            Work = "Engineer"
        };
        
        var createdPerson = PersonMother.CreateValidPerson();
        createdPerson.Id = 1;
        createdPerson.Name = createDto.Name;
        createdPerson.Age = createDto.Age;
        createdPerson.Address = createDto.Address;
        createdPerson.Work = createDto.Work;
        
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Person>())).ReturnsAsync(createdPerson);

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonCreatedHttpDto>>(result);
        var createdResult = Assert.IsType<CreatedResult>(actionResult.Result);
        
        Assert.Equal(201, createdResult.StatusCode);
        
        var dto = Assert.IsType<PersonCreatedHttpDto>(createdResult.Value);
        Assert.Equal(createdPerson.Id, dto.Id);
        Assert.Equal(createdPerson.Name, dto.Name);
        _mockService.Verify(s => s.CreateAsync(It.IsAny<Person>()), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid person (empty name) - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePerson_InvalidPerson_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto
        {
            Name = "", // Invalid
            Age = 30
        };
        
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonCreatedHttpDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
        Assert.Contains("Name", dto.Errors.Keys);
    }

    /// <summary>
    /// EP3: Person already exists - should return 409 Conflict
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePerson_PersonAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto
        {
            Name = "John Doe",
            Age = 30
        };
        
        var exception = new core.exceptions.dataaccess.repositories.PersonAlreadyExistsException(1);
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Person>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonCreatedHttpDto>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonAlreadyExistsHttpDto>(conflictResult.Value);
        
        Assert.Equal(409, dto.StatusCode);
        Assert.Equal(1, dto.PersonId);
    }

    /// <summary>
    /// EP4: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreatePerson_ServiceError_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto
        {
            Name = "John Doe",
            Age = 30
        };
        
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Person>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPersonServiceException>(
            () => _controller.CreatePerson(createDto)
        );
    }

    #endregion

    #region UpdatePerson Tests

    /// <summary>
    /// EP1: Valid person, Person exists - should return 200 OK with updated Person
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePerson_ValidPerson_PersonExists_ShouldReturnOk()
    {
        // Arrange
        var personId = 1;
        var updateDto = new UpdatePersonHttpDto
        {
            Id = personId,
            Name = "Updated Name",
            Age = 35
        };
        
        var updatedPerson = PersonMother.CreateValidPerson();
        updatedPerson.Id = personId;
        updatedPerson.Name = updateDto.Name;
        updatedPerson.Age = updateDto.Age;
        
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Person>())).ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.UpdatePerson(personId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonHttpDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonHttpDto>(okResult.Value);
        
        Assert.Equal(personId, dto.Id);
        Assert.Equal(updateDto.Name, dto.Name);
        _mockService.Verify(s => s.UpdateAsync(It.IsAny<Person>()), Times.Once);
    }

    /// <summary>
    /// EP2: Person not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePerson_PersonNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 999;
        var updateDto = new UpdatePersonHttpDto
        {
            Id = personId,
            Name = "Updated Name"
        };
        
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Person>()))
            .ThrowsAsync(new PersonNotFoundException(personId));

        // Act
        var result = await _controller.UpdatePerson(personId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonHttpDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonNotFoundHttpDto>(notFoundResult.Value);
        
        Assert.Equal(personId, dto.PersonId);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Invalid person (empty name) - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePerson_InvalidPerson_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var personId = 1;
        var updateDto = new UpdatePersonHttpDto
        {
            Id = personId,
            Name = "" // Invalid
        };
        
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdatePerson(personId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonHttpDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: ID mismatch - should return 400 Bad Request
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePerson_IdMismatch_ShouldReturnBadRequest()
    {
        // Arrange
        var personId = 1;
        var updateDto = new UpdatePersonHttpDto
        {
            Id = 999, // Different from route ID
            Name = "Updated Name"
        };

        // Act
        var result = await _controller.UpdatePerson(personId, updateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PersonHttpDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        
        Assert.Equal(400, dto.StatusCode);
        Assert.Contains("Id", dto.Errors.Keys);
    }

    /// <summary>
    /// EP5: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdatePerson_ServiceError_ShouldThrowException()
    {
        // Arrange
        var personId = 1;
        var updateDto = new UpdatePersonHttpDto
        {
            Id = personId,
            Name = "Updated Name"
        };
        
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Person>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPersonServiceException>(
            () => _controller.UpdatePerson(personId, updateDto)
        );
    }

    #endregion

    #region DeletePerson Tests

    /// <summary>
    /// EP1: Valid ID, Person exists - should return 200 OK
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePerson_ValidId_PersonExists_ShouldReturnOk()
    {
        // Arrange
        var personId = 1;
        _mockService.Setup(s => s.DeleteAsync(personId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePerson(personId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        _mockService.Verify(s => s.DeleteAsync(personId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Person not found - should return 404 Not Found
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePerson_ValidId_PersonNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 999;
        _mockService.Setup(s => s.DeleteAsync(personId))
            .ThrowsAsync(new PersonNotFoundException(personId));

        // Act
        var result = await _controller.DeletePerson(personId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<PersonNotFoundHttpDto>(notFoundResult.Value);
        
        Assert.Equal(personId, dto.PersonId);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP3: Service throws exception - should return 500 Internal Server Error
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeletePerson_ServiceError_ShouldThrowException()
    {
        // Arrange
        var personId = 1;
        _mockService.Setup(s => s.DeleteAsync(personId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpPersonServiceException>(
            () => _controller.DeletePerson(personId)
        );
    }

    #endregion
}
