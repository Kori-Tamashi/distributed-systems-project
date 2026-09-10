using core.domain;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Person entity
/// Provides predefined, reusable test data for common scenarios
/// Uses PersonBuilder for consistent object creation
/// </summary>
public static class PersonMother
{
    private static readonly PersonBuilder _defaultBuilder = new PersonBuilder();

    /// <summary>
    /// Creates a valid Person with all required fields
    /// </summary>
    public static Person CreateValidPerson()
    {
        return _defaultBuilder
            .WithId(1)
            .WithName("John Doe")
            .WithAge(30)
            .WithAddress("123 Main Street, New York, NY 10001")
            .WithWork("Software Engineer")
            .Build();
    }

    /// <summary>
    /// Creates a Person with minimal data (only required fields)
    /// </summary>
    public static Person CreateMinimalPerson()
    {
        return _defaultBuilder
            .WithId(2)
            .WithName("Jane Doe")
            .WithoutAge()
            .WithoutAddress()
            .WithoutWork()
            .Build();
    }

    /// <summary>
    /// Creates a Person with null optional fields
    /// </summary>
    public static Person CreatePersonWithNulls()
    {
        return _defaultBuilder
            .WithId(3)
            .WithName("Bob Smith")
            .WithoutAge()
            .WithoutAddress()
            .WithoutWork()
            .Build();
    }

    /// <summary>
    /// Creates a Person with empty string values
    /// </summary>
    public static Person CreatePersonWithEmptyStrings()
    {
        return _defaultBuilder
            .WithId(4)
            .WithName("")
            .WithAge(0)
            .WithAddress("")
            .WithWork("")
            .Build();
    }

    /// <summary>
    /// Creates a Person with maximum age value
    /// </summary>
    public static Person CreatePersonWithMaxAge()
    {
        return _defaultBuilder
            .WithId(5)
            .WithName("Elder Person")
            .WithAge(150)
            .WithAddress("123 Old Street")
            .WithWork("Retired")
            .Build();
    }

    /// <summary>
    /// Creates a Person with minimum age value
    /// </summary>
    public static Person CreatePersonWithMinAge()
    {
        return _defaultBuilder
            .WithId(6)
            .WithName("Baby")
            .WithAge(0)
            .WithAddress("123 Nursery Lane")
            .WithoutWork()
            .Build();
    }

    /// <summary>
    /// Creates a Person with invalid data (empty name)
    /// </summary>
    public static Person CreateInvalidPersonWithEmptyName()
    {
        return _defaultBuilder
            .WithId(7)
            .WithName("")
            .WithAge(25)
            .WithAddress("123 Test St")
            .WithWork("Tester")
            .Build();
    }

    /// <summary>
    /// Creates a Person with invalid data (negative age)
    /// </summary>
    public static Person CreateInvalidPersonWithNegativeAge()
    {
        return _defaultBuilder
            .WithId(8)
            .WithName("Invalid Person")
            .WithAge(-5)
            .WithAddress("123 Test St")
            .WithWork("Tester")
            .Build();
    }

    /// <summary>
    /// Creates a Person with invalid data (age over 150)
    /// </summary>
    public static Person CreateInvalidPersonWithExcessiveAge()
    {
        return _defaultBuilder
            .WithId(9)
            .WithName("Methuselah")
            .WithAge(969)
            .WithAddress("123 Ancient Road")
            .WithWork("Patriarch")
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Persons for collection tests
    /// </summary>
    public static List<Person> CreatePersonList(int count = 5)
    {
        var builder = new PersonBuilder();
        return builder.BuildList(count, incrementIds: true);
    }

    /// <summary>
    /// Creates a Person with unique random Id
    /// </summary>
    public static Person CreatePersonWithRandomId()
    {
        return new PersonBuilder()
            .WithName("Random Person")
            .WithAge(25)
            .WithAddress("Random Address")
            .WithWork("Random Work")
            .BuildWithRandomId();
    }
}
