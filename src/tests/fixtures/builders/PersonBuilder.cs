using core.domain;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Person entity
/// Allows fluent construction of Person objects for testing
/// </summary>
public class PersonBuilder
{
    private int _id = 1;
    private string _name = "John Doe";
    private int? _age = 30;
    private string? _address = "123 Main St";
    private string? _work = "Software Engineer";

    /// <summary>
    /// Sets the Person Id
    /// </summary>
    public PersonBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Person Name
    /// </summary>
    public PersonBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the Person Age
    /// </summary>
    public PersonBuilder WithAge(int age)
    {
        _age = age;
        return this;
    }

    /// <summary>
    /// Sets the Person Age to null
    /// </summary>
    public PersonBuilder WithoutAge()
    {
        _age = null;
        return this;
    }

    /// <summary>
    /// Sets the Person Address
    /// </summary>
    public PersonBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    /// <summary>
    /// Sets the Person Address to null
    /// </summary>
    public PersonBuilder WithoutAddress()
    {
        _address = null;
        return this;
    }

    /// <summary>
    /// Sets the Person Work
    /// </summary>
    public PersonBuilder WithWork(string work)
    {
        _work = work;
        return this;
    }

    /// <summary>
    /// Sets the Person Work to null
    /// </summary>
    public PersonBuilder WithoutWork()
    {
        _work = null;
        return this;
    }

    /// <summary>
    /// Builds the Person object with current configuration
    /// </summary>
    /// <returns>Person entity with configured properties</returns>
    public Person Build()
    {
        return new Person
        {
            Id = _id,
            Name = _name,
            Age = _age,
            Address = _address,
            Work = _work
        };
    }

    /// <summary>
    /// Builds a list of Person objects
    /// </summary>
    /// <param name="count">Number of persons to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each person</param>
    /// <returns>List of Person entities</returns>
    public List<Person> BuildList(int count, bool incrementIds = true)
    {
        var persons = new List<Person>();
        
        for (int i = 0; i < count; i++)
        {
            var person = new Person
            {
                Id = incrementIds ? _id + i : _id,
                Name = _name,
                Age = _age,
                Address = _address,
                Work = _work
            };
            persons.Add(person);
        }

        return persons;
    }

    /// <summary>
    /// Creates a Person with random Id
    /// </summary>
    public Person BuildWithRandomId()
    {
        return new Person
        {
            Id = new Random().Next(1, int.MaxValue),
            Name = _name,
            Age = _age,
            Address = _address,
            Work = _work
        };
    }
}
