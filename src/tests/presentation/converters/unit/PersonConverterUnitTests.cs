using core.domain;
using presentation.converters;
using presentation.dto.http;
using tests.config.attributes;
using tests.fixtures.mothers;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for PersonConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain (PersonHttpDto):
/// - EP1: Valid person with all fields (normal case)
/// - EP2: Person with null optional fields
/// - EP3: Person with minimal data
/// - EP4: Person with maximum values
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid person to create DTO
/// - EP2: Create DTO with null optional fields
/// - EP3: Create DTO should have Id = 0
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid person to update DTO
/// - EP2: Update DTO preserves ID
/// 
/// For ToListDTO:
/// - EP1: Convert list of persons
/// - EP2: Pagination metadata correct
/// 
/// For ToCreatedDTO:
/// - EP1: Convert with location header
/// - EP2: StatusCode is 201
/// 
/// For Error DTOs (ToNotFoundDTO, ToValidationDTO, ToAlreadyExistsDTO):
/// - EP1: Create error DTO with correct message
/// - EP2: StatusCode is correct for each error type
/// 
/// Total: 16 unit tests (all should pass)
/// </summary>
public class PersonConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid person with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ValidPersonWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();

        // Act
        var dto = PersonConverter.ToDTO(person);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(person.Id, dto.Id);
        Assert.Equal(person.Name, dto.Name);
        Assert.Equal(person.Age, dto.Age);
        Assert.Equal(person.Address, dto.Address);
        Assert.Equal(person.Work, dto.Work);
    }

    /// <summary>
    /// EP2: Person with null optional fields - should handle nulls
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_PersonWithNullOptionalFields_ShouldHandleNulls()
    {
        // Arrange
        var person = PersonMother.CreatePersonWithNulls();

        // Act
        var dto = PersonConverter.ToDTO(person);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(person.Id, dto.Id);
        Assert.Equal(person.Name, dto.Name);
        Assert.Null(dto.Age);
        Assert.Null(dto.Address);
        Assert.Null(dto.Work);
    }

    /// <summary>
    /// EP3: ToDomain should convert DTO back to domain entity
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ValidDto_ShouldConvertToDomainEntity()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        var dto = new PersonHttpDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Address = person.Address,
            Work = person.Work
        };

        // Act
        var result = PersonConverter.ToDomain(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Age, result.Age);
        Assert.Equal(dto.Address, result.Address);
        Assert.Equal(dto.Work, result.Work);
    }

    /// <summary>
    /// EP4: Round-trip conversion should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();

        // Act
        var dto = PersonConverter.ToDTO(person);
        var result = PersonConverter.ToDomain(dto);

        // Assert
        Assert.Equal(person.Id, result.Id);
        Assert.Equal(person.Name, result.Name);
        Assert.Equal(person.Age, result.Age);
        Assert.Equal(person.Address, result.Address);
        Assert.Equal(person.Work, result.Work);
    }

    /// <summary>
    /// EP5: ToDTO list should convert all persons
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfPersons_ShouldConvertAll()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);

        // Act
        var dtos = PersonConverter.ToDTO(persons);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(persons.Count, dtos.Count);
        for (int i = 0; i < persons.Count; i++)
        {
            Assert.Equal(persons[i].Id, dtos[i].Id);
            Assert.Equal(persons[i].Name, dtos[i].Name);
        }
    }

    /// <summary>
    /// EP6: ToDomain list should convert all DTOs
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ListOfDtos_ShouldConvertAll()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);
        var dtos = PersonConverter.ToDTO(persons);

        // Act
        var result = PersonConverter.ToDomain(dtos);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(persons.Count, result.Count);
        for (int i = 0; i < persons.Count; i++)
        {
            Assert.Equal(persons[i].Id, result[i].Id);
            Assert.Equal(persons[i].Name, result[i].Name);
        }
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid person to create DTO should map all fields
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidPerson_ShouldMapAllFields()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();

        // Act
        var dto = PersonConverter.ToCreateDTO(person);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(person.Name, dto.Name);
        Assert.Equal(person.Age, dto.Age);
        Assert.Equal(person.Address, dto.Address);
        Assert.Equal(person.Work, dto.Work);
    }

    /// <summary>
    /// EP2: ToCreateDomain should set Id to 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldSetIdToZero()
    {
        // Arrange
        var dto = new CreatePersonHttpDto
        {
            Name = "Test Person",
            Age = 25,
            Address = "Test Address",
            Work = "Test Work"
        };

        // Act
        var person = PersonConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, person.Id);
        Assert.Equal(dto.Name, person.Name);
    }

    /// <summary>
    /// EP3: Create DTO round-trip should preserve data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();

        // Act
        var dto = PersonConverter.ToCreateDTO(person);
        var result = PersonConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, result.Id);
        Assert.Equal(person.Name, result.Name);
        Assert.Equal(person.Age, result.Age);
        Assert.Equal(person.Address, result.Address);
        Assert.Equal(person.Work, result.Work);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid person to update DTO should map all fields including Id
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidPerson_ShouldMapAllFieldsIncludingId()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();

        // Act
        var dto = PersonConverter.ToUpdateDTO(person);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(person.Id, dto.Id);
        Assert.Equal(person.Name, dto.Name);
        Assert.Equal(person.Age, dto.Age);
        Assert.Equal(person.Address, dto.Address);
        Assert.Equal(person.Work, dto.Work);
    }

    /// <summary>
    /// EP2: ToUpdateDomain should preserve the Id
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveId()
    {
        // Arrange
        var dto = new UpdatePersonHttpDto
        {
            Id = 42,
            Name = "Updated Name",
            Age = 35,
            Address = "Updated Address",
            Work = "Updated Work"
        };

        // Act
        var person = PersonConverter.ToUpdateDomain(dto);

        // Assert
        Assert.Equal(42, person.Id);
        Assert.Equal(dto.Name, person.Name);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: ToListDTO should convert persons and set pagination
    /// </summary>
    [Fact]
    [Unit]
    public void ToListDTO_Persons_ShouldConvertAndSetPagination()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(10);
        int totalCount = 100;
        int page = 1;
        int pageSize = 10;

        // Act
        var dto = PersonConverter.ToListDTO(persons, totalCount, page, pageSize);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(10, dto.Items.Count);
        Assert.Equal(totalCount, dto.TotalCount);
        Assert.Equal(page, dto.Page);
        Assert.Equal(pageSize, dto.PageSize);
        Assert.Equal(10, dto.TotalPages);
        Assert.True(dto.HasNextPage);
        Assert.True(dto.HasPreviousPage);
    }

    /// <summary>
    /// EP2: ToListDTO first page should have no previous page
    /// </summary>
    [Fact]
    [Unit]
    public void ToListDTO_FirstPage_ShouldHaveNoPreviousPage()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);

        // Act
        var dto = PersonConverter.ToListDTO(persons, 5, 0, 5);

        // Assert
        Assert.False(dto.HasPreviousPage);
        Assert.False(dto.HasNextPage); // First page is also last page when total = page size
    }

    #endregion

    #region ToCreatedDTO Tests

    /// <summary>
    /// EP1: ToCreatedDTO should map person and location
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreatedDTO_ValidPerson_ShouldMapPersonAndLocation()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        var location = "/persons/1";

        // Act
        var dto = PersonConverter.ToCreatedDTO(person, location);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(person.Id, dto.Id);
        Assert.Equal(person.Name, dto.Name);
        Assert.Equal(location, dto.Location);
        Assert.Equal(201, dto.StatusCode);
    }

    #endregion

    #region Error DTO Tests

    /// <summary>
    /// EP1: ToNotFoundDTO should create DTO with correct message and status
    /// </summary>
    [Fact]
    [Unit]
    public void ToNotFoundDTO_ValidId_ShouldCreateCorrectDto()
    {
        // Arrange
        int personId = 42;

        // Act
        var dto = PersonConverter.ToNotFoundDTO(personId);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(personId, dto.PersonId);
        Assert.Contains("42", dto.Message);
        Assert.Equal(404, dto.StatusCode);
    }

    /// <summary>
    /// EP2: ToValidationDTO should create DTO with errors and status
    /// </summary>
    [Fact]
    [Unit]
    public void ToValidationDTO_Errors_ShouldCreateCorrectDto()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required" } },
            { "Age", new[] { "Age must be between 0 and 150" } }
        };
        var message = "Validation failed";

        // Act
        var dto = PersonConverter.ToValidationDTO(errors, message);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(2, dto.Errors.Count);
        Assert.Equal(message, dto.Message);
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP3: ToAlreadyExistsDTO should create DTO with correct message and status
    /// </summary>
    [Fact]
    [Unit]
    public void ToAlreadyExistsDTO_ValidId_ShouldCreateCorrectDto()
    {
        // Arrange
        int personId = 42;

        // Act
        var dto = PersonConverter.ToAlreadyExistsDTO(personId);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(personId, dto.PersonId);
        Assert.Contains("42", dto.Message);
        Assert.Equal(409, dto.StatusCode);
    }

    #endregion
}
