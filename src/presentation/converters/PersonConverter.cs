using presentation.dto.http;

namespace presentation.converters;

/// <summary>
/// Converter for Person between domain and DTO representations
/// </summary>
public static class PersonConverter
{
    /// <summary>
    /// Converts domain Person to PersonHttpDto
    /// </summary>
    public static PersonHttpDto ToDTO(core.domain.Person person)
    {
        return new PersonHttpDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Address = person.Address,
            Work = person.Work
        };
    }

    /// <summary>
    /// Converts PersonHttpDto to domain Person
    /// </summary>
    public static core.domain.Person ToDomain(PersonHttpDto dto)
    {
        return new core.domain.Person
        {
            Id = dto.Id,
            Name = dto.Name,
            Age = dto.Age,
            Address = dto.Address,
            Work = dto.Work
        };
    }

    /// <summary>
    /// Converts list of domain Persons to list of PersonHttpDtos
    /// </summary>
    public static List<PersonHttpDto> ToDTO(List<core.domain.Person> persons)
    {
        return persons.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts list of PersonHttpDtos to list of domain Persons
    /// </summary>
    public static List<core.domain.Person> ToDomain(List<PersonHttpDto> dtos)
    {
        return dtos.Select(ToDomain).ToList();
    }

    /// <summary>
    /// Converts domain Person to CreatePersonHttpDto
    /// </summary>
    public static CreatePersonHttpDto ToCreateDTO(core.domain.Person person)
    {
        return new CreatePersonHttpDto
        {
            Name = person.Name,
            Age = person.Age,
            Address = person.Address,
            Work = person.Work
        };
    }

    /// <summary>
    /// Converts CreatePersonHttpDto to domain Person
    /// </summary>
    public static core.domain.Person ToCreateDomain(CreatePersonHttpDto dto)
    {
        return new core.domain.Person
        {
            Id = 0,
            Name = dto.Name,
            Age = dto.Age,
            Address = dto.Address,
            Work = dto.Work
        };
    }

    /// <summary>
    /// Converts domain Person to UpdatePersonHttpDto
    /// </summary>
    public static UpdatePersonHttpDto ToUpdateDTO(core.domain.Person person)
    {
        return new UpdatePersonHttpDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Address = person.Address,
            Work = person.Work
        };
    }

    /// <summary>
    /// Converts UpdatePersonHttpDto to domain Person
    /// </summary>
    public static core.domain.Person ToUpdateDomain(UpdatePersonHttpDto dto)
    {
        return new core.domain.Person
        {
            Id = dto.Id,
            Name = dto.Name,
            Age = dto.Age,
            Address = dto.Address,
            Work = dto.Work
        };
    }

    /// <summary>
    /// Converts domain persons to paginated PersonListHttpDto
    /// </summary>
    public static PersonListHttpDto ToListDTO(
        List<core.domain.Person> persons,
        int totalCount,
        int page,
        int pageSize)
    {
        return new PersonListHttpDto
        {
            Items = persons.Select(ToDTO).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Converts domain Person to PersonCreatedHttpDto
    /// </summary>
    public static PersonCreatedHttpDto ToCreatedDTO(core.domain.Person person, string location)
    {
        return new PersonCreatedHttpDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Address = person.Address,
            Work = person.Work,
            Location = location
        };
    }

    /// <summary>
    /// Creates PersonNotFoundHttpDto from person ID
    /// </summary>
    public static PersonNotFoundHttpDto ToNotFoundDTO(int personId)
    {
        return new PersonNotFoundHttpDto
        {
            PersonId = personId,
            Message = $"Person with ID {personId} was not found"
        };
    }

    /// <summary>
    /// Creates PersonValidationHttpDto from errors
    /// </summary>
    public static PersonValidationHttpDto ToValidationDTO(
        Dictionary<string, string[]> errors,
        string message)
    {
        return new PersonValidationHttpDto
        {
            Errors = errors,
            Message = message
        };
    }

    /// <summary>
    /// Creates PersonAlreadyExistsHttpDto from person ID
    /// </summary>
    public static PersonAlreadyExistsHttpDto ToAlreadyExistsDTO(int personId)
    {
        return new PersonAlreadyExistsHttpDto
        {
            PersonId = personId,
            Message = $"Person with ID {personId} already exists"
        };
    }
}
